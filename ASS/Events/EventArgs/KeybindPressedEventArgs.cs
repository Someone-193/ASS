namespace ASS.Events.EventArgs
{
    using System;

    using ASS.Features.Settings;

    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class KeybindPressedEventArgs(Player player, ASSKeybind keybind) : EventArgs, IPlayerEvent
    {
        public Player Player { get; set; } = player;

        public ASSKeybind Keybind { get; set; } = keybind;
    }
}