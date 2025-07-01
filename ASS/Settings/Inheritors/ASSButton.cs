namespace ASS.Settings.Inheritors
{
    using System;
    using LabApi.Features.Wrappers;
    using UserSettings.ServerSpecific;

    public class ASSButton : ASSBase
    {
        public ASSButton(int id, string? buttonLabel = null, string? buttonText = null, float holdTime = 0, string? hint = null)
        {
            Id = id;
            Label = buttonLabel;
            ButtonText = buttonText;
            HoldTime = holdTime;
            Hint = hint;
        }

        public string? ButtonText { get; set; }

        public float HoldTime { get; set; }

        public override Type SSSType { get; } = typeof(SSButton);

        internal override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.ChangeOnly;

        protected internal virtual void OnPressed(Player sender, ASSBase setting)
        {
        }
    }
}