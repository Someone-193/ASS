namespace ASS.Example.PlayerMenuExamples
{
    using System.Collections.Generic;

    #if EXILED
    using Player = Exiled.API.Features.Player;
    #endif

    using ASS.Events.EventArgs;
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
            {
                if (Menus.TryGetValue(ev.Player, out PlayerMenu menu1))
                    menu1.Destroy();

                return;
            }

            if (!Menus.TryGetValue(ev.Player, out PlayerMenu menu2) && ServerStatic.PermissionsHandler.IsRaPermitted(ev.Group.Permissions))
            {
                Menus[ev.Player] = new PlayerMenu(Generator, ev.Player, true);
            }
            else if (!ServerStatic.PermissionsHandler.IsRaPermitted(ev.Group.Permissions))
            {
                menu2?.Destroy();
            }
        }

        public static void OnLeft(PlayerLeftEventArgs ev)
        {
            if (Menus.TryGetValue(ev.Player, out PlayerMenu menu))
                menu.Destroy();
        }

        public static void OnSettingTriggered(SettingTriggeredEventArgs ev)
        {
            if (ev.Setting.Id is -13 && Valid(ev.Player))
            {
                ev.Player.IsGodModeEnabled = !ev.Player.IsGodModeEnabled;
            }
        }

        private static ASSGroup Generator(Player owner)
        {
            return new ASSGroup(
                [
                    new ASSButton(-13, "Toggle Godmode", "Toggle", 1F, "Toggles godmode")
                ],
                1,
                p => p == owner && Valid(p));
        }

        private static bool Valid(Player player)
        {
            #if EXILED
            return ServerStatic.PermissionsHandler.IsRaPermitted(player.Group?.Permissions ?? 0);
            #else
            return ServerStatic.PermissionsHandler.IsRaPermitted(player.UserGroup?.Permissions ?? 0);
            #endif
        }
    }
}