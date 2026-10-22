namespace ASS.Features.Settings
{
    using System;
    using System.Collections.Generic;

    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif

    using LabApi.Features.Wrappers;

    using Mirror;

    using UserSettings.ServerSpecific;

    public class ASSTwoButtonsDisplay : ASSBase
    {
        private bool rightSelected;

        private string leftOption;
        private string rightOption;

        public ASSTwoButtonsDisplay(
            int id,
            string? label = null,
            string leftOption = "",
            string rightOption = "",
            bool rightSelected = false,
            string? hint = null)
        {
            Id = id;
            Label = label;
            this.leftOption = leftOption;
            this.rightOption = rightOption;
            Hint = hint;

            this.rightSelected = rightSelected;
        }

        public bool LeftSelected
        {
            get
            {
                return !rightSelected;
            }

            set
            {
                rightSelected = !value;
                UpdateValue(!value, this.SettingHolders());
            }
        }

        public bool RightSelected
        {
            get
            {
                return rightSelected;
            }

            set
            {
                rightSelected = value;
                UpdateValue(value, this.SettingHolders());
            }
        }

        public string LeftOption
        {
            get => leftOption;
            set
            {
                leftOption = value;
                if (AutoSync && IsInstance)
                    UpdateTwoButtons(this.SettingHolders());
            }
        }

        public string RightOption
        {
            get => rightOption;
            set
            {
                rightOption = value;
                if (AutoSync && IsInstance)
                    UpdateTwoButtons(this.SettingHolders());
            }
        }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSTwoButtonsSetting);

        public static implicit operator ASSTwoButtonsDisplay(SSTwoButtonsSetting twoButtons) => new(twoButtons.SettingId, twoButtons.Label, twoButtons.OptionA, twoButtons.OptionB, twoButtons.DefaultIsB, twoButtons.HintDescription);

        public static implicit operator SSTwoButtonsSetting(ASSTwoButtonsDisplay twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.LeftOption, twoButtons.RightOption, twoButtons.RightSelected, twoButtons.Hint, twoButtons.CollectionId, true);

        #if EXILED
        public static implicit operator ASSTwoButtonsDisplay(TwoButtonsSetting twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.FirstOption, twoButtons.SecondOption, twoButtons.IsSecondDefault, twoButtons.HintDescription)
        {
            ExHeader = twoButtons.Header,
            ExAction = twoButtons.OnChanged,
        };

        public static implicit operator TwoButtonsSetting(ASSTwoButtonsDisplay twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.LeftOption, twoButtons.RightOption, twoButtons.RightSelected, twoButtons.Hint, twoButtons.CollectionId, true, twoButtons.ExHeader, twoButtons.ExAction);
        #endif

        public void UpdateValue(bool newRightSelected, IEnumerable<Player>? players)
        {
            UpdateDerived(
                writer =>
                {
                    writer.WriteByte(1);
                    writer.WriteBool(newRightSelected);
                },
                players);
        }

        public void UpdateLeftOption(string newLeftOption, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(newLeftOption, RightOption), players);
        }

        public void UpdateRightOption(string newRightOption, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(LeftOption, newRightOption), players);
        }

        public void UpdateTwoButtons(IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(LeftOption, RightOption), players);
        }

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteString(LeftOption);
            writer.WriteString(RightOption);
            writer.WriteBool(RightSelected);
        }

        internal override void Deserialize(NetworkReaderPooled reader)
        {
            rightSelected = reader.ReadBool();

            base.Deserialize(reader);
        }

        internal override ASSBase Copy() => new ASSTwoButtonsDisplay(Id, Label, LeftOption, RightOption, RightSelected, Hint);

        private static Action<NetworkWriter> GetAction(string newLeftOption, string newRightOption)
        {
            return writer =>
            {
                writer.WriteByte(2);
                writer.WriteString(newLeftOption);
                writer.WriteString(newRightOption);
            };
        }
    }
}