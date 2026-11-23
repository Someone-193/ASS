namespace ASS.Events.EventArgs
{
    using System;
    using ASS.Features.Instances;
    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class UpdatingInstanceEventArgs(Player player, ASSInstance instance) : EventArgs, IPlayerEvent, ICancellableEvent
    {
        public Player Player { get; set; } = player;

        public ASSInstance Instance { get; set; } = instance;

        public bool IsAllowed { get; set; } = true;
    }
}