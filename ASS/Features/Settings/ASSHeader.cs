namespace ASS.Features.Settings
{
    using System;
    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif
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

        public static implicit operator ASSHeader(SSGroupHeader header) => new(header.Label, header.HintDescription);

        public static implicit operator SSGroupHeader(ASSHeader header) => new(header.Label, false, header.Hint);

        #if EXILED
        public static implicit operator ASSHeader(HeaderSetting header) => new(header.Label, header.HintDescription);

        public static implicit operator HeaderSetting(ASSHeader header) => new(header.Label, header.Hint);
        #endif

        internal override ASSBase Copy() => new ASSHeader(Label, Hint);
    }
}