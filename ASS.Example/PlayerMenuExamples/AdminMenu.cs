namespace ASS.Example.PlayerMenuExamples
{
    using System.Collections.Generic;
    using ASS.Features.Collections;
    using ASS.Features.Settings;
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Features.Wrappers;

    public class AdminMenu
    {
        private static readonly Dictionary<Player, PlayerMenu> Menus = new();

        public static void OnChangedGroup(PlayerGroupChangedEventArgs ev)
        {
            // NW moment
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (ev.Group is null)
                return;

            if (!Menus.TryGetValue(ev.Player, out PlayerMenu menu) && ServerStatic.PermissionsHandler.IsRaPermitted(ev.Group.Permissions))
            {
                Menus[ev.Player] = new PlayerMenu(Generator, ev.Player);
            }
            else
            {
                menu?.Destroy();
            }
        }

        public static void OnLeft(PlayerLeftEventArgs ev)
        {
            if (Menus.TryGetValue(ev.Player, out PlayerMenu menu))
                menu.Destroy();
        }

        public static void OnSettingTriggered(Player sender, ASSBase setting)
        {
            if (setting.Id is -13 && Valid(sender))
            {
                sender.IsGodModeEnabled = !sender.IsGodModeEnabled;
            }
        }

        private static ASSGroup Generator(Player owner)
        {
            return new ASSGroup(
                [
                    new ASSButton(-13, "Toggle Godmode", "Toggle", 1F, "Toggles godmode")
                ],
                1,
                Valid);
        }

        private static bool Valid(Player player)
        {
            return ServerStatic.PermissionsHandler.IsRaPermitted(player.UserGroup?.Permissions ?? 0);
        }
    }
}