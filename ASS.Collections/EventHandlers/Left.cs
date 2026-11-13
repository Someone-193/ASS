namespace ASS.Collections.EventHandlers
{
    using ASS.Collections.Features.Pages;

    using LabApi.Events.Arguments.PlayerEvents;

    internal static class Left
    {
        public static void OnLeft(PlayerLeftEventArgs ev) => ASSPage.SelectedPage.Remove(ev.Player);
    }
}