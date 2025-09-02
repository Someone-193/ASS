namespace ASS.EventHandlers
{
    using ASS.Features;

    using LabApi.Events.Arguments.PlayerEvents;

    public class Left
    {
        public static void OnLeft(PlayerLeftEventArgs ev)
        {
            ASSNetworking.FixedPlayers.Remove(ev.Player.ReferenceHub);
            ASSNetworking.ReceivedSettings.Remove(ev.Player);
        }
    }
}