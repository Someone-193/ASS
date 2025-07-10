namespace ASS.EventHandlers
{
    using ASS.Settings;
    using LabApi.Events.Arguments.PlayerEvents;
    using MEC;

    public class Joined
    {
        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            // gives ASS priority on joining syncs
            Timing.CallDelayed(0, () => ASSNetworking.SendToPlayerFull(ev.Player, true, false, true, true));
        }
    }
}