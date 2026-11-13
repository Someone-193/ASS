namespace ASS.Features.Templates
{
    using System;

    using ASS.Features.Attributes;
    using ASS.Features.Instances;

    using UserSettings.ServerSpecific;

    [CanSync]
    public partial class TwoButtonsTemplate : ASSTemplate
    {
        public InstanceHandler<TwoButtonsInstance>? OnChanged { get; set; }

        public string LeftOption { get; set; } = string.Empty;

        public string RightOption { get; set; } = string.Empty;

        public bool DefaultRightSelected { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSTwoButtonsSetting);
    }
}