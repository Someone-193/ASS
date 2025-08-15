namespace ASS.Events.EventArgs
{
    using System;

    using ASS.Features.Settings;

    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class ButtonPressedEventArgs(Player player, ASSButton button) : EventArgs, IPlayerEvent
    {
        public Player Player { get; set; } = player;

        public ASSButton Button { get; set; } = button;
    }
}