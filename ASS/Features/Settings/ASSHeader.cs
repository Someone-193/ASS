namespace ASS.Features.Settings
{
    using System;
    using UserSettings.ServerSpecific;

    public class ASSHeader : ASSBase
    {
        public ASSHeader(string? label = null, string? hint = null)
        {
            Label = label;
            Hint = hint;
        }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.None;

        internal override Type SSSType { get; } = typeof(SSGroupHeader);

        internal override ASSBase Copy() => new ASSHeader(Label, Hint);
    }
}