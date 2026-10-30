namespace ASS.Features.Templates
{
    using System;

    using ASS.Features.Attributes;
    using ASS.Features.Instances;

    using UserSettings.ServerSpecific;

    [CanSync]
    public partial class ButtonTemplate : ASSTemplate
    {
        public InstanceHandler<ButtonInstance>? OnPressed { get; set; }

        public string ButtonText { get; set; } = string.Empty;

        public float HoldTime { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.ChangeOnly;

        internal override Type SSSType { get; } = typeof(SSButton);
    }
}