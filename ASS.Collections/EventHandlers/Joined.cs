namespace ASS.Collections.EventHandlers
{
    using System.Collections.Generic;

    using ASS.Collections.Features.Pages;

    using LabApi.Events.Arguments.PlayerEvents;

    internal static class Joined
    {
        public static void OnJoined(PlayerJoinedEventArgs ev) => ASSPage.SelectedPage[ev.Player] = new Stack<ASSPage>();
    }
}