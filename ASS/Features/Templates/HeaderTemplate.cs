namespace ASS.Features.Templates
{
    using System;

    using UserSettings.ServerSpecific;

    public partial class HeaderTemplate : ASSTemplate
    {
        public bool ReducedPadding { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.None;

        internal override Type SSSType { get; } = typeof(SSGroupHeader);
    }
}