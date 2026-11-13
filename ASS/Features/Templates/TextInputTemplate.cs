namespace ASS.Features.Templates
{
    using System;

    using ASS.Features.Attributes;
    using ASS.Features.Instances;

    using TMPro;

    using UserSettings.ServerSpecific;

    [CanSync]
    public partial class TextInputTemplate : ASSTemplate
    {
        public InstanceHandler<TextInputInstance>? OnTextInputted { get; set; }

        public string DefaultText { get; set; } = string.Empty;

        public string Placeholder { get; set; } = "...";

        public ushort CharacterLimit { get; set; } = 64;

        public TMP_InputField.ContentType ContentType { get; set; } = TMP_InputField.ContentType.Standard;

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSPlaintextSetting);
    }
}