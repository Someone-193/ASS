namespace ASS.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using ASS.Events.EventArgs;
    using ASS.Events.Handlers;
    using ASS.Features.Collections;
    using ASS.Features.MirrorUtils;
    using ASS.Features.MirrorUtils.Messages;
    using ASS.Features.Settings;
    using ASS.Features.Settings.Displays;

    using NorthwoodLib.Pools;

    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif

    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;

    using Mirror;

    using UserSettings.ServerSpecific;

    public static class ASSNetworking
    {
        internal static readonly HashSet<ReferenceHub> FixedPlayers = [];

        private static readonly Dictionary<ReferenceHub, Action> QueuedUpdates = new();

        /// <summary>
        /// Gets a dictionary assigning an <see cref="Array"/> of <see cref="ASSBase"/> to every player. Gets updated before settings are sent.
        /// </summary>
        public static Dictionary<Player, ASSBase[]> ReceivedSettings { get; } = new();

        public static List<ASSGroup> Groups { get; } = [];

        public static IEnumerable<ASSBase> Settings { get; } = Groups.SelectMany(group => group.GetAllSettings());

        public static Dictionary<Player, int> Versions { get; } = new();

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="ASSBase"/> containing all settings that will be sent to clients when verified, before displaying by groups.
        /// </summary>
        /// <remarks>
        /// This is useful if you hide certain options behind other settings (like a scrollable tab for groups of settings) and you want to read all stored client values for all settings before going back to your tab based approach.
        /// </remarks>
        public static List<ASSBase> InitializingSettings { get; } = [];

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="ASSKeybind"/> containing all keybinds that will be visible below all other settings no matter what groups are visible.
        /// </summary>
        /// <remarks>
        /// Useful if you hide your keybinds behind other settings (say you bind keybinds when you set a <see cref="ASSDropdown"/> to "Keybinds") but you want to maintain functionality.
        /// </remarks>
        public static List<ASSKeybind> PersistantKeybinds { get; } = [];

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> containing overrides for players, meaning if an entry for a given player exists here, SendToPlayer will only send settings from the groups in that entry.
        /// </summary>
        /// <remarks>
        /// Useful if you want to force only a certain group or 2 to be visible to a player (like a setting controlling what settings you can see, then the group of settings you're looking at).
        /// </remarks>
        public static Dictionary<Player, ASSGroup[]> PlayerOverrides { get; } = new();

        public static bool TryGetSetting<T>(Player player, int id, [NotNullWhen(true)] out T? value)
            where T : ASSBase
        {
            value = !ReceivedSettings.TryGetValue(player, out ASSBase[] settings) ? null : settings.FirstOrDefault(setting => setting.Id == id) as T;
            return value is not null;
        }

        public static void SendToAll()
        {
            foreach (Player player in Player.ReadyList)
            {
                SendToPlayer(player);
            }
        }

        public static void SendToPlayer(Player player)
        {
            SendToPlayerFull(player);
        }

        public static void SendToPlayerFull(Player player, bool includeBaseGameSettings = true, bool registerChange = true, bool forceLoad = false, bool ignoreResponses = false, ASSBase[]? responseOverride = null)
        {
            SendCustomToPlayer(player, GetRegisteredSorted(player).ToArray(), includeBaseGameSettings, registerChange, forceLoad, ignoreResponses, responseOverride);
        }

        // main stuff happening here is we're queuing the actual message if the target doesn't have their SSS tab open, and sending only the necessary settings (A "Loading..." Header and all keybinds) to minimize lag by only sending the queued message once they open up their SSS tab while maintaining seamless functionality
        public static void SendCustomToPlayer(Player player, ASSBase[] settings, bool includeBaseGameSettings = true, bool registerChange = true, bool forceLoad = false, bool ignoreResponses = false, ASSBase[]? responseOverride = null)
        {
            if (!NetworkServer.active)
                return;

            List<ASSBase> list = Copy(settings);

            if (ignoreResponses)
            {
                foreach (ASSBase setting in list)
                {
                    setting.IgnoreNextResponse = setting.ResponseMode is ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange && !(responseOverride?.Contains(setting) ?? false);
                }
            }

            if (forceLoad || player.TabOpen())
            {
                Logger.Debug($"Sending {settings.Length} settings to {player.Nickname} via {nameof(SendCustomToPlayer)}", Main.Debug);
                ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack(list, includeBaseGameSettings ? ServerSpecificSettingsSync.DefinedSettings ?? [] : [], GetVersion(player)));
            }
            else
            {
                Logger.Debug($"Sending {settings.Length} (pre-minimizing) settings to {player.Nickname} via {nameof(SendCustomToPlayer)}", Main.Debug);
                ASSUtils.SendASSMessage(player.Connection, MinimizedPack(list, includeBaseGameSettings ? ServerSpecificSettingsSync.DefinedSettings ?? [] : [], GetVersion(player, registerChange)));
                QueuedUpdates[player.ReferenceHub] = () =>
                {
                    if (ignoreResponses)
                    {
                        foreach (ASSBase setting in list)
                        {
                            setting.IgnoreNextResponse = setting.ResponseMode is ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange && !(responseOverride?.Contains(setting) ?? false);
                        }
                    }

                    Logger.Debug($"Sending {settings.Length} settings to {player.Nickname} via {nameof(SendCustomToPlayer)} after tab was opened", Main.Debug);
                    ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack(list, includeBaseGameSettings ? ServerSpecificSettingsSync.DefinedSettings ?? [] : [], GetVersion(player)));
                };
            }
        }

        public static void SendSSSIncludingASS(Player player, ServerSpecificSettingBase[] settings, int? version, bool forceLoad = false)
        {
            if (!NetworkServer.active)
                return;

            if (!ReceivedSettings.TryGetValue(player, out ASSBase[] value))
                ReceivedSettings[player] = value = [];

            if (forceLoad || player.TabOpen())
                ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack(ListPool<ASSBase>.Shared.Rent(value), settings, GetVersion(player)));
            else
            {
                if (!QueuedUpdates.ContainsKey(player.ReferenceHub))
                {
                    Logger.Debug($"Sending {settings.Length} settings (pre-minimizing) to {player.Nickname} via {nameof(SendSSSIncludingASS)}", Main.Debug);
                    ASSUtils.SendASSMessage(player.Connection, MinimizedPack(value, settings, version ?? GetVersion(player)));
                }

                QueuedUpdates[player.ReferenceHub] = () =>
                {
                    Logger.Debug($"Sending {settings.Length} settings to {player.Nickname} via {nameof(SendSSSIncludingASS)} after tab was opened", Main.Debug);
                    ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack(ListPool<ASSBase>.Shared.Rent(value), settings, version ?? GetVersion(player)));
                };
            }
        }

        public static void RegisterGroups(IEnumerable<ASSGroup> groups, IEnumerable<Player>? toUpdate = null)
        {
            groups = groups.Where(group => group != null);
            Groups.AddRange(groups);

            if (toUpdate is null)
                return;

            foreach (Player player in toUpdate)
                SendToPlayer(player);
        }

        public static void UnregisterGroups(IEnumerable<ASSGroup> groups, IEnumerable<Player>? toUpdate = null)
        {
            groups = groups.Where(group => group != null);
            Groups.RemoveAll(groups.Contains);

            if (toUpdate is null)
                return;

            foreach (Player player in toUpdate)
                SendToPlayer(player);
        }

        public static int GetVersion(Player player, bool newVersion = false)
        {
            if (Versions.TryGetValue(player, out int val))
            {
                if (!newVersion)
                    return val;

                val++;
                Versions[player] = val;

                return val;
            }

            Versions[player] = ServerSpecificSettingsSync.Version;
            return ServerSpecificSettingsSync.Version;
        }

        public static IEnumerable<Player> SettingHolders(this ASSBase setting) => ReceivedSettings.Where(kvp => kvp.Value.Contains(setting)).Select(kvp => kvp.Key);

        #if EXILED
        internal static Action<Player, ASSBase> Convert(this Action<Exiled.API.Features.Player, SettingBase> action)
        {
            return (player, setting) =>
            {
                action(player, setting);
            };
        }
        #endif

        /// <summary>
        /// Creates an <see cref="ASSEntriesPack"/> containing only a singular header and all keybinds from the provided settings.
        /// </summary>
        /// <param name="assSettings">The ASS settings to search for keybinds.</param>
        /// <param name="baseGameSettings">The ServerSpecificSetting settings to search for keybinds.</param>
        /// <param name="version">What version to send this pack with.</param>
        /// <returns>A minimized <see cref="ASSEntriesPack"/>.</returns>
        internal static ASSEntriesPack MinimizedPack(IEnumerable<ASSBase> assSettings, IEnumerable<ServerSpecificSettingBase> baseGameSettings, int version)
        {
            return new ASSEntriesPack(
                ListPool<ASSBase>.Shared.Rent(assSettings
                    .Where(setting => setting.SSSType == typeof(SSKeybindSetting))
                    .Prepend(new ASSHeader("Plz No Hash Collision".GetStableHashCode(), "Loading...", false, "ASS is currently loading all of your settings!"))),
                baseGameSettings
                    .Where(setting => setting.GetType() == typeof(SSKeybindSetting))
                    .ToArray(),
                version);
        }

        internal static void SendByFilter(Func<ReferenceHub, bool> filter)
        {
            foreach (ReferenceHub hub in ReferenceHub.AllHubs.Where(filter))
            {
                SendToPlayer(Player.Get(hub));
            }
        }

        internal static IEnumerable<ASSBase> GetRegisteredSorted(Player player)
        {
            IEnumerable<ASSGroup> groups;

            if (PlayerOverrides.TryGetValue(player, out ASSGroup[] groupArray))
            {
                groups = groupArray;
            }
            else
            {
                groups = Groups;
            }

            return groups.OrderByDescending(group => group.Priority).SelectMany(group => group.GetViewableSettingsOrdered(player));
        }

        internal static void ProcessResponseMessage(NetworkConnectionToClient conn, SSSClientResponse message)
        {
            try
            {
                ServerSpecificSettingsSync.ServerProcessClientResponseMsg(conn, message);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to process SSSClientResponse in base game!\n{ex}");
            }

            if (!ReferenceHub.TryGetHub(conn, out ReferenceHub hub))
                return;

            Player p = Player.Get(hub);

            ASSBase? setting = ReceivedSettings.TryGetValue(p, out ASSBase[] settings) ? settings.FirstOrDefault(setting => setting.Id == message.Id) : null;

            if (setting is null)
            {
                return;
            }

            Logger.Debug("Received ASS setting response", Main.Debug);

            setting.Deserialize(NetworkReaderPool.Get(message.Payload));

            if (setting.IgnoreNextResponse)
            {
                setting.IgnoreNextResponse = false;
                return;
            }

            Logger.Debug("Running events", Main.Debug);

            SettingEvents.OnSettingTriggered(new SettingTriggeredEventArgs(p, setting));

            switch (setting)
            {
                case ASSKeybind keybind:
                    SettingEvents.OnKeybindPressed(new KeybindPressedEventArgs(p, keybind));
                    break;
                case ASSDropdown dropdown:
                    SettingEvents.OnDropdownTriggered(new DropdownTriggeredEventArgs(p, dropdown));
                    break;
                case ASSTwoButtons twoButtons:
                    SettingEvents.OnTwoButtonsPressed(new TwoButtonsPressedEventArgs(p, twoButtons));
                    break;
                case ASSSlider slider:
                    SettingEvents.OnSliderMoved(new SliderMovedEventArgs(p, slider));
                    break;
                case ASSButton button:
                    SettingEvents.OnButtonPressed(new ButtonPressedEventArgs(p, button));
                    break;
                case ASSTextInput textInput:
                    SettingEvents.OnTextInputChanged(new TextInputChangedEventArgs(p, textInput));
                    break;
                case ASSDisplay:
                    Logger.Warn($"Received setting from ASS display [{setting}]! Please verify you are handling your settings correctly.");
                    break;
                default:
                    Logger.Warn($"Failed to cast setting [{setting}]");
                    break;
            }
        }

        internal static void OnStatusReceived(ReferenceHub hub, SSSUserStatusReport report)
        {
            if (report.TabOpen && FixedPlayers.Add(hub))
            {
                Player p = Player.Get(hub);

                SendToPlayerFull(p, true, false, true);
            }

            if (report.TabOpen && QueuedUpdates.TryGetValue(hub, out Action update))
            {
                QueuedUpdates.Remove(hub);
                update();
            }
        }

        private static List<ASSBase> Copy(ASSBase[] toCopy)
        {
            List<ASSBase> val = ListPool<ASSBase>.Shared.Rent(toCopy.Length);

            for (int i = 0; i < toCopy.Length; i++)
            {
                val.Add(toCopy[i].Copy());
                val[i].IsInstance = true;
            }

            return val;
        }
    }
}