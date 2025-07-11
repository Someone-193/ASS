namespace ASS.Features.Settings
{
    using System;
    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif
    using LabApi.Features.Wrappers;
    using Mirror;
    using UserSettings.ServerSpecific;

    public class ASSButton : ASSBase
    {
        public ASSButton(int id, string? buttonLabel = null, string? buttonText = null, float holdTime = 0, string? hint = null, Action<Player, ASSBase>? onChanged = null)
        {
            Id = id;
            Label = buttonLabel;
            ButtonText = buttonText;
            HoldTime = holdTime;
            Hint = hint;
            OnChanged = onChanged;
        }

        public string? ButtonText { get; set; }

        public float HoldTime { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.ChangeOnly;

        internal override Type SSSType { get; } = typeof(SSButton);

        public static implicit operator ASSButton(SSButton button) => new(button.SettingId, button.Label, button.ButtonText, button.HoldTimeSeconds, button.HintDescription);

        public static implicit operator SSButton(ASSButton button) => new(button.Id, button.Label, button.ButtonText, button.HoldTime, button.Hint);

        #if EXILED
        public static implicit operator ASSButton(ButtonSetting button) => new(button.Id, button.Label, button.Text, button.HoldTime, button.HintDescription, button.OnChanged.Convert())
        {
            ExHeader = button.Header,
            ExAction = button.OnChanged,
        };

        public static implicit operator ButtonSetting(ASSButton button) => new(button.Id, button.Label, button.ButtonText, button.HoldTime, button.Hint, button.ExHeader, button.ExAction);
        #endif

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteFloat(HoldTime);
            writer.WriteString(ButtonText);
        }

        internal override ASSBase Copy() => new ASSButton(Id, Label, ButtonText, HoldTime, Hint);
    }
}