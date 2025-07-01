namespace ASS.Settings.Inheritors
{
    using System;
    using UserSettings.ServerSpecific;

    public class ASSHeader : ASSBase
    {
        public ASSHeader(string label, string? hint = null, bool reducePadding = false)
        {
            Label = label;
            Hint = hint;
            ReducePadding = reducePadding;
        }

        public bool ReducePadding { get; set; }

        public override Type SSSType { get; } = typeof(SSGroupHeader);

        internal override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.None;
    }
}