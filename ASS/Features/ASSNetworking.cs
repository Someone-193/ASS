namespace ASS.Features
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using ASS.EventHandlers;
    using ASS.Events.EventArgs;
    using ASS.Events.Handlers;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.MirrorUtils;
    using ASS.Features.MirrorUtils.Messages;
    using ASS.Features.Templates;

    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;

    using Mirror;

    using NorthwoodLib.Pools;

    using UserSettings.ServerSpecific;

    public static class ASSNetworking
    {
        internal static readonly HashSet<ReferenceHub> FixedPlayers = [];
        internal static readonly HashSet<ReferenceHub> DirtyPlayers = [];
        internal static readonly Dictionary<IASSObject, int> ObjectsValue = new();

        private static readonly Dictionary<ReferenceHub, Action> QueuedUpdates = new();

        /// <summary>
        /// Gets a dictionary assigning an <see cref="Array"/> of <see cref="ASSTemplate"/> to every player. Gets updated before settings are sent.
        /// </summary>
        public static Dictionary<Player, ASSInstance[]> SentSettings { get; } = new();

        public static ReadOnlyDictionary<IASSObject, int> Objects { get; } = new(ObjectsValue);

        public static Dictionary<Player, int> Versions { get; } = new();

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="ASSTemplate"/> containing all settings that will be sent to clients when verified, before displaying by groups.
        /// </summary>
        /// <remarks>
        /// This is useful if you hide certain options behind other settings (like a scrollable tab for groups of settings) and you want to read all stored client values for all settings before going back to your tab based approach.
        /// </remarks>
        public static HashSet<ASSTemplate> InitializingSettings { get; } = [];

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="KeybindTemplate"/> containing all keybinds that will be visible below all other settings no matter what groups are visible.
        /// </summary>
        /// <remarks>
        /// Useful if you hide your keybinds behind other settings (say you bind keybinds when you set a <see cref="DropdownTemplate"/> to "Keybinds") but you want to maintain functionality.
        /// </remarks>
        public static HashSet<KeybindTemplate> PersistantKeybinds { get; } = [];

        /// <summary>
        /// Gets or sets the generator used to get the root settings of the hierarchy shown to a player.
        /// </summary>
        /// <remarks>tldr; for base ASS, just <see cref="Objects"/>, sorted. For ASS.Pages, gets the players selected page.</remarks>
        public static Func<Player, IEnumerable<IASSObject>> TemplateRootGenerator { get; set; } = Main.StandardTemplateRootGenerator;

        /// <summary>
        /// Gets or sets the default behavior for sending base game settings for a player.
        /// </summary>
        /// <remarks>tldr; for base ASS, always true. For ASS.Pages, only true when the player isn't in a custom page.</remarks>
        public static Func<Player, bool> SendBaseGameSettingsGenerator { get; set; } = Main.StandardSendBaseGameSettingsGenerator;

        public static bool TryGetSetting<T>(Player player, int id, [NotNullWhen(true)] out T? value)
            where T : ASSInstance
            => (value = !SentSettings.TryGetValue(player, out ASSInstance[] settings) ? null : settings.FirstOrDefault(setting => setting.Id == id) as T) is not null;

        public static bool TryGetSetting<T>(Player player, string[] path, [NotNullWhen(true)] out T? value)
            where T : ASSInstance
            => (value = !SentSettings.TryGetValue(player, out ASSInstance[] settings) ? null : settings.FirstOrDefault(setting => setting.Id == ASSExtensions.GenerateId(path)) as T) is not null;

        /// <summary>
        /// A helper method that updates a players settings once they open their SSS tab. This method is the ideal way to send a player new settings. Though if keybinds are involved, the standard <see cref="SendToPlayer"/> should be used.
        /// </summary>
        /// <param name="player">The player to send settings, once they open up their SSS tab.</param>
        public static void SetDirty(Player player)
        {
            if (player.TabOpen())
            {
                SendToPlayerFull(player, null, false, true);
                return;
            }

            DirtyPlayers.Add(player.ReferenceHub);
        }

        public static void SendToAll()
        {
            foreach (Player player in Player.ReadyList)
                SendToPlayer(player);
        }

        public static void SendToPlayer(Player player) => SendToPlayerFull(player);

        public static void SendToPlayerFull(Player player, bool? includeBaseGameSettings = null, bool registerChange = true, bool forceLoad = false) => SendCustomToPlayer(player, TemplateRootGenerator.Invoke(player), includeBaseGameSettings, registerChange, forceLoad);

        // main stuff happening here is we're queuing the actual message if the target doesn't have their SSS tab open, and sending only the necessary settings (A "Loading..." Header and all keybinds) to minimize lag by only sending the queued message once they open up their SSS tab while maintaining seamless functionality
        public static void SendCustomToPlayer(Player player, IEnumerable<IASSObject> root, bool? includeBaseGameSettings = null, bool registerChange = true, bool forceLoad = false)
        {
            if (Joined.Locked.Contains(player) || !CheckInitialized(player))
                return;

            ServerSpecificSettingBase[] baseGame = includeBaseGameSettings.HasValue
                ? includeBaseGameSettings.Value ? ServerSpecificSettingsSync.DefinedSettings ?? [] : []
                : SendBaseGameSettingsGenerator(player) ? ServerSpecificSettingsSync.DefinedSettings ?? [] : [];

            List<ASSInstance> builtSettings = ASSExtensions.BuildSettings(root, player);

            if (forceLoad || player.TabOpen())
            {
                Logger.Debug($"Sending {builtSettings.Count} settings to {player.Nickname} via {nameof(SendCustomToPlayer)}", Main.Debug);
                ASSUtils.SendASSMessage(player, new ASSEntriesPack(builtSettings, baseGame, GetVersion(player)));
            }
            else
            {
                Logger.Debug($"Sending {builtSettings.Count} (pre-minimizing) settings to {player.Nickname} via {nameof(SendCustomToPlayer)}", Main.Debug);
                ASSUtils.SendASSMessage(player, MinimizedPack(player, builtSettings, baseGame, GetVersion(player, registerChange)));

                QueuedUpdates[player.ReferenceHub] = () =>
                {
                    Logger.Debug($"Sending {builtSettings.Count} settings to {player.Nickname} via {nameof(SendCustomToPlayer)} after tab was opened", Main.Debug);
                    ASSUtils.SendASSMessage(player, new ASSEntriesPack(builtSettings, baseGame, GetVersion(player)));
                };
            }
        }

        public static void SendSSSIncludingASS(Player player, ServerSpecificSettingBase[] settings, int? version, bool forceLoad = false)
        {
            if (Joined.Locked.Contains(player) || !CheckInitialized(player))
                return;

            if (!SentSettings.TryGetValue(player, out ASSInstance[] value))
                SentSettings[player] = value = [];

            if (forceLoad || player.TabOpen())
                ASSUtils.SendASSMessage(player, new ASSEntriesPack(ListPool<ASSInstance>.Shared.Rent(value), settings, GetVersion(player)));
            else
            {
                Logger.Debug($"Sending {settings.Length} settings (pre-minimizing) to {player.Nickname} via {nameof(SendSSSIncludingASS)}", Main.Debug);
                ASSUtils.SendASSMessage(player, MinimizedPack(player, ListPool<ASSInstance>.Shared.Rent(value), settings, version ?? GetVersion(player)));

                QueuedUpdates[player.ReferenceHub] = () =>
                {
                    Logger.Debug($"Sending {settings.Length} settings to {player.Nickname} via {nameof(SendSSSIncludingASS)} after tab was opened", Main.Debug);
                    ASSUtils.SendASSMessage(player, new ASSEntriesPack(ListPool<ASSInstance>.Shared.Rent(value), settings, version ?? GetVersion(player)));
                };
            }
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

        /// <summary>
        /// Creates an <see cref="ASSEntriesPack"/> containing only a singular header and all keybinds from the provided settings.
        /// </summary>
        /// <param name="viewer">The viewer.</param>
        /// <param name="assSettings">The ASS settings to search for keybinds.</param>
        /// <param name="baseGameSettings">The ServerSpecificSetting settings to search for keybinds.</param>
        /// <param name="version">What version to send this pack with.</param>
        /// <returns>A minimized <see cref="ASSEntriesPack"/>.</returns>
        internal static ASSEntriesPack MinimizedPack(Player viewer, IEnumerable<ASSInstance> assSettings, IEnumerable<ServerSpecificSettingBase> baseGameSettings, int version)
        {
            return new ASSEntriesPack(
                ListPool<ASSInstance>.Shared.Rent(assSettings
                    .Where(setting => setting.Template.SSSType == typeof(SSKeybindSetting))
                    .Prepend(new HeaderInstance(viewer, "Plz No Hash Collision".GetStableHashCode(), HeaderTemplate.Create(string.Empty).WithLabel("Loading...").WithHint("ASS is currently loading all of your settings!")))),
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

        internal static void ProcessResponseMessage(NetworkConnectionToClient conn, SSSClientResponse message)
        {
            Logger.Debug($"Received SSS client response from {message.Id}", Main.Debug);

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

            Player player = Player.Get(hub);

            if (!SentSettings.TryGetValue(player, out ASSInstance[]? settings))
                return;

            ASSInstance? instance = null;
            for (int i = 0; i < settings.Length; i++)
            {
                if (settings[i].Id != message.Id)
                    continue;

                instance = settings[i];
                break;
            }

            if (instance is null)
                return;

            if (Joined.Locked.Count > 0)
                CheckJoined(player, settings);

            Logger.Debug("Processing ASS setting response", Main.Debug);

            instance.TimesTriggered++;
            instance.Deserialize(NetworkReaderPool.Get(message.Payload));

            Logger.Debug("Running events", Main.Debug);

            SettingEvents.OnSettingTriggered(new InstanceTriggeredEventArgs(player, instance));
            switch (instance)
            {
                case ButtonInstance button:
                    button.OnPressed?.Invoke(player, button);
                    break;
                case DropdownInstance dropdown:
                    dropdown.OnDropdownChanged?.Invoke(player, dropdown);
                    break;
                case KeybindInstance keybind:
                    keybind.OnChanged?.Invoke(player, keybind);
                    break;
                case SliderInstance slider:
                    slider.OnChanged?.Invoke(player, slider);
                    break;
                case TextInputInstance textInput:
                    textInput.OnTextInputted?.Invoke(player, textInput);
                    break;
                case TwoButtonsInstance twoButtons:
                    twoButtons.OnChanged?.Invoke(player, twoButtons);
                    break;
            }
        }

        internal static void OnStatusReceived(ReferenceHub hub, SSSUserStatusReport report)
        {
            if (!report.TabOpen)
                return;

            if (QueuedUpdates.Remove(hub, out Action update))
            {
                update();
                return;
            }

            if (DirtyPlayers.Remove(hub) || FixedPlayers.Add(hub))
                SendToPlayerFull(Player.Get(hub), null, false, true);
        }

        private static bool CheckInitialized(Player player) => !(Main.Instance?.Config?.ForceJoinSyncPriority ?? true) || Joined.InitializedPlayers.Contains(player);

        // fancy way of doing IEnumerable::All() but without LINQ.
        private static void CheckJoined(Player p, ASSInstance[] instances)
        {
            bool remove = true;
            for (int i = 0; i < instances.Length; i++)
            {
                if (instances[i].InitialResponseReceived)
                    continue;

                remove = false;
                break;
            }

            if (remove)
                Joined.Locked.Remove(p);
        }
    }
}