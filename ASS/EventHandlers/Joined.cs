namespace ASS.EventHandlers
{
    using ASS.Features;

    using LabApi.Events.Arguments.PlayerEvents;

    using MEC;

    public class Joined
    {
        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            // gives ASS priority on joining syncs
            Timing.CallDelayed(0, () =>
            {
                // send all settings that need to be read from client, but might not be typically visible.
                if (ASSNetworking.InitializingSettings.Count != 0)
                    ASSNetworking.SendCustomToPlayer(ev.Player, ASSNetworking.InitializingSettings.ToArray(), false, false, true);

                // send default settings
                ASSNetworking.SendToPlayerFull(ev.Player, true, false, true);
            });
        }
    }
}