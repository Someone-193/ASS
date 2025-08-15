namespace ASS.Events.EventArgs
{
    using ASS.Features.Settings;

    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class SettingSentEventArgs(Player player, ASSBase setting) : System.EventArgs, IPlayerEvent
    {
        public Player Player { get; set; } = player;

        public ASSBase Setting { get; set; } = setting;
    }
}