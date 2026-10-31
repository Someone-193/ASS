namespace ASS.Example.PlayerMenuExamples
{
    using System.Collections.Generic;

    using ASS.Features.Containers;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Features.Wrappers;

    public class AdminMenu
    {
        public static PlayerMenu Menu { get; } = PlayerMenu.Create("Admin Menu", Generator).WithCondition(Valid);

        public static void OnChangedGroup(PlayerGroupChangedEventArgs ev)
        {
            // NW moment
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (ev.Group is null)
            {
                Menu.Remove(ev.Player);
                return;
            }

            if (!Menu.Children.ContainsKey(ev.Player) && ServerStatic.PermissionsHandler.IsRaPermitted(ev.Group.Permissions))
                Menu.Add(ev.Player);
            else if (!ServerStatic.PermissionsHandler.IsRaPermitted(ev.Group.Permissions))
                Menu.Remove(ev.Player);
        }

        public static void OnLeft(PlayerLeftEventArgs ev) => Menu.Remove(ev.Player, false);

        private static IEnumerable<IASSObject> Generator(Player p)
        {
            yield return ButtonTemplate
                .Create("Toggle")
                .WithHandler(Pressed)
                .WithLabel("Toggle Godmode")
                .WithButtonText("Toggle")
                .WithHoldTime(1)
                .WithHint("Toggles godmode");
        }

        private static void Pressed(Player player, ButtonInstance button)
        {
            // technically redundant as buttons do not have an initial response.
            if (button.IsInitialResponse)
                return;

            if (!Valid(player))
                return;

            player.IsGodModeEnabled = !player.IsGodModeEnabled;
        }

        private static bool Valid(Player player) => ServerStatic.PermissionsHandler.IsRaPermitted(player.UserGroup?.Permissions ?? 0);
    }
}