namespace ASS.Features.Settings
{
    using System;
    using Mirror;
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

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.ChangeOnly;

        internal override Type SSSType { get; } = typeof(SSButton);

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteFloat(HoldTime);
            writer.WriteString(ButtonText);
        }

        internal override ASSBase Copy() => new ASSButton(Id, Label, ButtonText, HoldTime, Hint);
    }
}