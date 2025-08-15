namespace ASS.Events.EventArgs
{
    using System;

    using ASS.Features.Settings;

    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class TextInputChangedEventArgs(Player player, ASSTextInput textInput) : EventArgs, IPlayerEvent
    {
        public Player Player { get; set; } = player;

        public ASSTextInput TextInput { get; set; } = textInput;
    }
}