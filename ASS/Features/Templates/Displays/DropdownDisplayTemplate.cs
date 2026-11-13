namespace ASS.Features.Templates.Displays
{
    using System;

    using UserSettings.ServerSpecific;

    public partial class DropdownDisplayTemplate : ASSDisplayTemplate
    {
        public byte IndexSelected { get; set; }

        public string[] Options { get; set; } = [string.Empty];

        public SSDropdownSetting.DropdownEntryType EntryType { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSDropdownSetting);
    }
}