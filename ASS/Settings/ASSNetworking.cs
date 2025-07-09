namespace ASS.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ASS.MirrorUtils;
    using ASS.MirrorUtils.Messages;
    using ASS.Settings.Inheritors;
    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;
    using Mirror;
    using UserSettings.ServerSpecific;

    public static class ASSNetworking
    {
        private static readonly Dictionary<ReferenceHub, Action> QueuedUpdates = new();

        public static event Action<Player, ASSBase> SettingTriggered = (_, _) => { };

        public static event Action<Player, ASSKeybind> KeybindPressed = (plyr, keybind) => keybind.OnPressed(plyr, keybind);

        public static event Action<Player, ASSDropdown> DropdownTriggered = (plyr, dropdown) => dropdown.OnTriggered(plyr, dropdown);

        public static event Action<Player, ASSTwoButtons> TwoButtonsPressed = (plyr, button) => button.OnPressed(plyr, button);

        public static event Action<Player, ASSSlider> SliderMoved = (plyr, slider) => slider.OnMoved(plyr, slider);

        public static event Action<Player, ASSButton> ButtonPressed = (plyr, button) => button.OnPressed(plyr, button);

        public static event Action<Player, ASSTextInput> TextInputChanged = (plyr, textInput) => textInput.OnChanged(plyr, textInput);

        public static Dictionary<Player, ASSBase[]> ReceivedSettings { get; } = new();

        public static List<ASSGroup> Groups { get; } = [];

        public static IEnumerable<ASSBase> Settings { get; } = Groups.SelectMany(group => group.GetAllSettings());

        public static Dictionary<Player, int> Versions { get; } = new();

        public static T? TryGetSetting<T>(Player player, int id)
            where T : ASSBase, new()
        {
            return !ReceivedSettings.TryGetValue(player, out ASSBase[] settings) ? null : settings.FirstOrDefault(setting => setting.Id == id) as T;
        }

        public static void SendToAll()
        {
            foreach (Player player in Player.ReadyList)
            {
                SendToPlayer(player);
            }
        }

        public static void SendToPlayer(Player player, bool includeBaseGameSettings = true, bool registerChange = true)
        {
            SendToPlayer(player, GetRegisteredSorted(player).ToArray(), includeBaseGameSettings, registerChange);
        }

        public static void SendToPlayer(Player player, ASSBase[] settings, bool includeBaseGameSettings = true, bool registerChange = true)
        {
            if (!NetworkServer.active)
                return;

            ReceivedSettings[player] = Copy(settings);

            foreach (ASSBase setting in ReceivedSettings[player])
            {
                if (setting.ResponseMode is ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange)
                    setting.IgnoreNextResponse = true;
            }

            if (player.TabOpen())
                ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack(settings, includeBaseGameSettings ? ServerSpecificSettingsSync.DefinedSettings : [], GetVersion(player)));
            else
            {
                if (!QueuedUpdates.ContainsKey(player.ReferenceHub))
                    ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack([new ASSHeader("Loading...")], null, GetVersion(player, registerChange)));
                QueuedUpdates[player.ReferenceHub] = () => ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack(settings, includeBaseGameSettings ? ServerSpecificSettingsSync.DefinedSettings : [], GetVersion(player)));
            }
        }

        public static void SendSSSIncludingASS(Player player, ServerSpecificSettingBase[] settings, int version)
        {
            if (player.TabOpen())
                ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack(ReceivedSettings[player], settings, GetVersion(player)));
            else
            {
                if (!QueuedUpdates.ContainsKey(player.ReferenceHub))
                    ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack([new ASSHeader("Loading...")], null, version));
                QueuedUpdates[player.ReferenceHub] = () => ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack(ReceivedSettings[player], settings, version));
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
            // TODO: Make this work lol
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

        internal static void SendByFilter(Func<ReferenceHub, bool> filter)
        {
            foreach (ReferenceHub hub in ReferenceHub.AllHubs.Where(filter))
            {
                SendToPlayer(Player.Get(hub));
            }
        }

        internal static IEnumerable<ASSBase> GetRegisteredSorted(Player player)
        {
            return Groups.OrderByDescending(group => group.Priority).SelectMany(group => group.GetViewableSettingsOrdered(player));
        }

        internal static void ProcessResponseMessage(NetworkConnectionToClient conn, SSSClientResponse message)
        {
            ServerSpecificSettingsSync.ServerProcessClientResponseMsg(conn, message);

            if (!ReferenceHub.TryGetHub(conn, out ReferenceHub hub))
                return;

            Player p = Player.Get(hub);

            ASSBase? setting = !ReceivedSettings.TryGetValue(p, out ASSBase[] settings) ? null : settings.FirstOrDefault(setting => setting.Id == message.Id);

            if (setting is null)
                return;

            setting.Deserialize(NetworkReaderPool.Get(message.Payload));

            if (setting.IgnoreNextResponse)
            {
                setting.IgnoreNextResponse = false;
                return;
            }

            SettingTriggered(p, setting);

            switch (setting)
            {
                case ASSKeybind keybind:
                    KeybindPressed(p, keybind);
                    break;
                case ASSDropdown dropdown:
                    DropdownTriggered(p, dropdown);
                    break;
                case ASSTwoButtons twoButtons:
                    TwoButtonsPressed(p, twoButtons);
                    break;
                case ASSSlider slider:
                    SliderMoved(p, slider);
                    break;
                case ASSButton button:
                    ButtonPressed(p, button);
                    break;
                case ASSTextInput textInput:
                    TextInputChanged(p, textInput);
                    break;
                default:
                    Logger.Warn($"Failed to cast setting [{setting}]");
                    break;
            }
        }

        internal static void OnStatusReceived(ReferenceHub hub, SSSUserStatusReport report)
        {
            if (report.TabOpen && QueuedUpdates.TryGetValue(hub, out Action update))
            {
                QueuedUpdates.Remove(hub);
                update();
            }
        }

        private static ASSBase[] Copy(ASSBase[] toCopy)
        {
            ASSBase[] val = new ASSBase[toCopy.Length];

            for (int i = 0; i < toCopy.Length; i++)
                val[i] = toCopy[i].Copy();

            return val;
        }
    }
}