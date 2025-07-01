namespace ASS.Example.PlayerMenuExamples
{
    using System.Collections.Generic;
    using ASS.Settings;
    using ASS.Settings.Inheritors;
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Features.Wrappers;

    public class WelcomeSetting
    {
        private static readonly Dictionary<Player, PlayerMenu> Menus = new();

        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            Menus[ev.Player] = new PlayerMenu(Generator, ev.Player);
        }

        public static void OnLeft(PlayerLeftEventArgs ev)
        {
            if (Menus.TryGetValue(ev.Player, out PlayerMenu menu))
                menu.Destroy();
        }

        private static ASSGroup Generator(Player owner)
        {
            return new ASSGroup([new ASSHeader($"Welcome {owner.DisplayName}!")], 100);
        }
    }
}