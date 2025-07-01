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

        public static event Action<Player, ASSButton> ButtonPressed = (plyr, button) => button.OnPressed(plyr, button);

        public static Dictionary<Player, ASSBase[]> SentSettings { get; } = new();

        public static List<ASSGroup> Groups { get; } = [];

        public static int Version { get; set; }

        public static void SendToAll()
        {
            foreach (Player player in Player.ReadyList)
            {
                SendToPlayer(player);
            }
        }

        public static void SendToPlayers(Predicate<Player> receivers)
        {
        }

        public static void SendToPlayer(Player player)
        {
            SendToPlayer(player, GetRegisteredSorted(player).ToArray());
        }

        public static void SendToPlayer(Player player, ASSBase[] settings)
        {
            SentSettings[player] = settings;

            ASSUtils.SendASSMessage(player.Connection, new ASSEntriesPack(settings, Version));
        }

        public static void RegisterGroups(IEnumerable<ASSGroup> groups, IEnumerable<Player> toUpdate)
        {
            groups = groups.Where(group => group != null);
            Groups.AddRange(groups);
            foreach (Player player in toUpdate)
                SendToPlayer(player);
        }

        public static void RegisterGroups(IEnumerable<ASSGroup> groups, Predicate<Player> toUpdate)
        {
            groups = groups.Where(group => group != null);
            Groups.AddRange(groups);
            SendToPlayers(toUpdate);
        }

        public static bool ValidateMessage(Player sender, ASSBase setting)
        {
            return false;
        }

        internal static IEnumerable<ASSBase> GetRegisteredSorted(Player player)
        {
            return Groups.OrderByDescending(group => group.Priority).SelectMany(group => group.GetViewableSettingsOrdered(player));
        }

        internal static void ProcessMessage(NetworkConnectionToClient conn, SSSClientResponse message)
        {
            if (!ReferenceHub.TryGetHub(conn, out ReferenceHub hub))
                return;

            Player p = Player.Get(hub);

            ASSBase? setting = SentSettings[p].FirstOrDefault(setting => setting.Id == message.Id);

            if (setting is null)
                return;

            if (setting.IgnoreNextResponse)
            {
                setting.IgnoreNextResponse = false;
                return;
            }

            SettingTriggered(p, setting);

            switch (ServerSpecificSettingsSync.GetCodeFromType(setting.SSSType))
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    if (setting is not ASSButton button)
                        throw new InvalidCastException($"Setting {setting} had an SSSType of button, but is not an ASSButton");

                    ButtonPressed(p, button);
                    break;
                case 7:
                    break;
            }
        }
    }
}