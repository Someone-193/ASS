namespace ASS.Features.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif

    using LabApi.Features.Wrappers;

    using Mirror;

    using UserSettings.ServerSpecific;

    public class ASSButton : ASSBase
    {
        private string buttonText = null!;
        private float holdTime;

        public ASSButton(int id, string? buttonLabel = null, string buttonText = "", float holdTime = 0, string? hint = null, Action<Player, ASSBase>? onChanged = null)
        {
            Id = id;
            Label = buttonLabel;
            ButtonText = buttonText;
            HoldTime = holdTime;
            Hint = hint;
            OnChanged = onChanged;
        }

        public string ButtonText
        {
            get => buttonText;
            set
            {
                buttonText = value;
                UpdateButton(this, ASSNetworking.ReceivedSettings.Where(kvp => kvp.Value.Contains(this)).Select(kvp => kvp.Key));
            }
        }

        public float HoldTime
        {
            get => holdTime;
            set
            {
                holdTime = value;
                UpdateButton(this, ASSNetworking.ReceivedSettings.Where(kvp => kvp.Value.Contains(this)).Select(kvp => kvp.Key));
            }
        }

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

        public static void UpdateButtonText(string buttonText, ASSButton button, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(buttonText, button.HoldTime), button, players);
        }

        public static void UpdateHoldTime(float holdTime, ASSButton button, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(button.ButtonText, holdTime), button, players);
        }

        public static void UpdateButton(ASSButton button, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(button.ButtonText, button.HoldTime), button, players);
        }

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteFloat(HoldTime);
            writer.WriteString(ButtonText);
        }

        internal override ASSBase Copy() => new ASSButton(Id, Label, ButtonText, HoldTime, Hint, OnChanged);

        private static Action<NetworkWriter> GetAction(string newText, float newHoldTime)
        {
            return writer =>
            {
                writer.WriteString(newText);
                writer.WriteFloat(newHoldTime);
            };
        }
    }
}