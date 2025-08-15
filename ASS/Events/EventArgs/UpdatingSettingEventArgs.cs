namespace ASS.Events.EventArgs
{
    using System;

    using ASS.Features.Settings;

    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class UpdatingSettingEventArgs(Player player, ASSBase setting) : EventArgs, IPlayerEvent, ICancellableEvent
    {
        public Player Player { get; set; } = player;

        public ASSBase Setting { get; set; } = setting;

        public bool IsAllowed { get; set; } = true;
    }
}