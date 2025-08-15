namespace ASS.Events.EventArgs
{
    using System;

    using ASS.Features.Settings;

    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class TwoButtonsPressedEventArgs(Player player, ASSTwoButtons twoButtons) : EventArgs, IPlayerEvent
    {
        public Player Player { get; set; } = player;

        public ASSTwoButtons TwoButtons { get; set; } = twoButtons;
    }
}