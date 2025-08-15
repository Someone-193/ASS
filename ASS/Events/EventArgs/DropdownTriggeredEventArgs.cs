namespace ASS.Events.EventArgs
{
    using System;

    using ASS.Features.Settings;

    using LabApi.Events.Arguments.Interfaces;
    using LabApi.Features.Wrappers;

    public class DropdownTriggeredEventArgs(Player player, ASSDropdown dropdown) : EventArgs, IPlayerEvent
    {
        public Player Player { get; set; } = player;

        public ASSDropdown Dropdown { get; set; } = dropdown;
    }
}