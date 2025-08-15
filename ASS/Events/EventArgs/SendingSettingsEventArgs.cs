namespace ASS.Events.EventArgs
{
    using System.Collections.Generic;

    using ASS.Features.Settings;

    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class SendingSettingsEventArgs(Player player, List<ASSBase> settings) : System.EventArgs, IPlayerEvent, ICancellableEvent
    {
        public Player Player { get; set; } = player;

        public List<ASSBase> Settings { get; set; } = settings;

        public bool IsAllowed { get; set; } = true;
    }
}