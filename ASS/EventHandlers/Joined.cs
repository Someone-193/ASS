namespace ASS.EventHandlers
{
    using ASS.Settings;
    using LabApi.Events.Arguments.PlayerEvents;

    public class Joined
    {
        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            ASSNetworking.SendToPlayerFull(ev.Player, registerChange: true, forceLoad: false);
        }
    }
}