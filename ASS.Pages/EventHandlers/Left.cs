namespace ASS.Pages.EventHandlers
{
    using ASS.Pages.Features;

    using LabApi.Events.Arguments.PlayerEvents;

    internal static class Left
    {
        public static void OnLeft(PlayerLeftEventArgs ev) => ASSPage.SelectedPage.Remove(ev.Player);
    }
}