namespace ASS.EventHandlers
{
    using ASS.Settings;
    using LabApi.Events.Arguments.PlayerEvents;

    public class Joined
    {
        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            ASSNetworking.SendToPlayer(ev.Player, registerChange: false, forceLoad: true);
        }
    }
}