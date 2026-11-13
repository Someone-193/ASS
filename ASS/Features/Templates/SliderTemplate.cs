namespace ASS.Features.Templates
{
    using System;

    using ASS.Features.Attributes;
    using ASS.Features.Instances;

    using UserSettings.ServerSpecific;

    [CanSync]
    public partial class SliderTemplate : ASSTemplate
    {
        public InstanceHandler<SliderInstance>? OnChanged { get; set; }

        public float DefaultValue { get; set; }

        public float MinValue { get; set; } = -10;

        public float MaxValue { get; set; } = 10;

        public bool IsInteger { get; set; }

        public string ValueFormat { get; set; } = "0.##";

        public string DisplayFormat { get; set; } = "{0}";

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSSliderSetting);
    }
}