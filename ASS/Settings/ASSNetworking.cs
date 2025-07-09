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

    public class ASSNetworking
    {
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

        public static int Version { get; set; }

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

        public static void SendToPlayer(Player player)
        {
            SendToPlayer(player, GetRegisteredSorted(player).ToArray());
        }

        public static void SendToPlayer(Player player, ASSBase[] settings, bool useBaseGameSettings = true, int version = -1)
        {
            if (version == -1)
                version = Version;

            ReceivedSettings[player] = Copy(settings);

            ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack(settings, useBaseGameSettings ? ServerSpecificSettingsSync.DefinedSettings : [], version));
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

            if (setting.IgnoreNextResponse)
            {
                setting.IgnoreNextResponse = false;
                return;
            }

            setting.Deserialize(NetworkReaderPool.Get(message.Payload));

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

        private static ASSBase[] Copy(ASSBase[] toCopy)
        {
            ASSBase[] val = new ASSBase[toCopy.Length];

            for (int i = 0; i < toCopy.Length; i++)
                val[i] = toCopy[i].Copy();

            return val;
        }
    }
}