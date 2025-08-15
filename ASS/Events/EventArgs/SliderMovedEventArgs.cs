namespace ASS.Events.EventArgs
{
    using System;

    using ASS.Features.Settings;

    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class SliderMovedEventArgs(Player player, ASSSlider slider) : EventArgs, IPlayerEvent
    {
        public Player Player { get; set; } = player;

        public ASSSlider Slider { get; set; } = slider;
    }
}