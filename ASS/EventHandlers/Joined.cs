namespace ASS.EventHandlers
{
    using System.Collections.Generic;

    using ASS.Features;

    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Features.Wrappers;

    using MEC;

    public class Joined
    {
        public static HashSet<Player> Locked { get; } = [];

        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            // gives ASS priority on joining syncs
            Timing.CallDelayed(0, () =>
            {
                // send all settings that need to be read from client, but might not be typically visible.
                if (ASSNetworking.InitializingSettings.Count != 0)
                {
                    ASSNetworking.SendCustomToPlayer(ev.Player, ASSNetworking.InitializingSettings.ToArray(), false, false, true);

                    // lock all setting sending until they send back settings from InitializingSettings
                    Locked.Add(ev.Player);

                    // send default settings
                    Timing.RunCoroutine(WaitUntilUnlocked(ev.Player));
                }
                else
                {
                    // if none exist, just send settings
                    ASSNetworking.SendToPlayerFull(ev.Player, true, false, true);
                }
            });
        }

        // ideally only runs a few ticks
        private static IEnumerator<float> WaitUntilUnlocked(Player player)
        {
            int ticks = 0;
            while (player.GameObject)
            {
                ticks++;

                yield return Timing.WaitForOneFrame;

                // skip check if > 8s have passed
                if (Locked.Contains(player) && ticks <= 60 * 8)
                    continue;

                ASSNetworking.SendToPlayerFull(player, true, false, true);
                break;
            }
        }
    }
}