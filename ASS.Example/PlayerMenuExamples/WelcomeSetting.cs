namespace ASS.Example.PlayerMenuExamples
{
    using System.Collections.Generic;
    using ASS.Features.Collections;
    using ASS.Features.Settings;
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Features.Wrappers;

    public class WelcomeSetting
    {
        private static readonly Dictionary<Player, PlayerMenu> Menus = new();

        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            AbstractExample.Instance.Add(ev.Player);
            Menus[ev.Player] = new PlayerMenu(Generator, ev.Player);
        }

        public static void OnLeft(PlayerLeftEventArgs ev)
        {
            AbstractExample.Instance.Remove(ev.Player);
            if (Menus.TryGetValue(ev.Player, out PlayerMenu menu))
                menu.Destroy();
        }

        private static ASSGroup Generator(Player owner)
        {
            return new ASSGroup(
            [
                new ASSHeader(-12, $"Welcome {owner.DisplayName}!"),
                new ASSButton(-10, "Test 1"),
                new ASSButton(-11, "Test 2"),
            ],
            5, 
            p => p == owner);
        }
    }
}