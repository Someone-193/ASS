namespace ASS.EventHandlers
{
    using ASS.Features;

    using LabApi.Events.Arguments.PlayerEvents;

    internal class Left
    {
        public static void OnLeft(PlayerLeftEventArgs ev)
        {
            ASSNetworking.FixedPlayers.Remove(ev.Player.ReferenceHub);
            ASSNetworking.SentSettings.Remove(ev.Player);
        }
    }
}