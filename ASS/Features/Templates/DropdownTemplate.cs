namespace ASS.Features.Templates
{
    using System;

    using ASS.Features.Attributes;
    using ASS.Features.Instances;

    using UserSettings.ServerSpecific;

    [CanSync]
    public partial class DropdownTemplate : ASSTemplate
    {
        public InstanceHandler<DropdownInstance>? OnDropdownChanged { get; set; }

        public string[] Options { get; set; } = [string.Empty];

        public byte DefaultIndex { get; set; }

        public SSDropdownSetting.DropdownEntryType EntryType { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSDropdownSetting);
    }
}