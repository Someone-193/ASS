namespace ASS.Events.EventArgs
{
    using System.Collections.Generic;
    using ASS.Events.Interfaces;
    using ASS.Features.Instances;
    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class SendingInstancesEventArgs(Player player, List<ASSInstance> settings) : IASSEventArgs, ICancellableEvent
    {
        public Player Player { get; set; } = player;

        public List<ASSInstance> Settings { get; } = settings;

        public bool IsAllowed { get; set; } = true;
    }
}