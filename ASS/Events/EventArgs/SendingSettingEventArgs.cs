namespace ASS.Events.EventArgs
{
    using ASS.Features.Settings;

    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class SendingSettingEventArgs(Player player, ASSBase setting) : System.EventArgs, IPlayerEvent, ICancellableEvent
    {
        public Player Player { get; set; } = player;

        public ASSBase Setting { get; set; } = setting;

        public bool IsAllowed { get; set; } = true;
    }
}