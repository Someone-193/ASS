namespace ASS.Features.Templates.Displays
{
    using System;

    using UserSettings.ServerSpecific;

    public partial class TwoButtonsDisplayTemplate : ASSDisplayTemplate
    {
        public bool LeftSelected
        {
            get => !RightSelected;
            set => RightSelected = !value;
        }

        public bool RightSelected { get; set; }

        public string LeftOption { get; set; } = string.Empty;

        public string RightOption { get; set; } = string.Empty;

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSTwoButtonsSetting);
    }
}