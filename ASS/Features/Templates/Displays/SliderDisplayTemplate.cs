namespace ASS.Features.Templates.Displays
{
    using System;

    using UserSettings.ServerSpecific;

    public partial class SliderDisplayTemplate : ASSDisplayTemplate
    {
        public float Value { get; set; }

        public float MinValue { get; set; }

        public float MaxValue { get; set; }

        public bool IsInteger { get; set; }

        public string ValueFormat { get; set; } = "0.##";

        public string DisplayFormat { get; set; } = "{0}";

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSSliderSetting);
    }
}