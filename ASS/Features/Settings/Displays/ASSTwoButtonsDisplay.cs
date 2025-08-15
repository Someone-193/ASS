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

    // TODO: fix n change stuff
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
            bool defaultRightSelected = false,
            string? hint = null,
            Action<Player, ASSBase>? onChanged = null,
            byte collectionId = byte.MaxValue)
        {
            Id = id;
            Label = label;
            this.leftOption = leftOption;
            this.rightOption = rightOption;
            DefaultRightSelected = defaultRightSelected;
            Hint = hint;
            OnChanged = onChanged;
            CollectionId = collectionId;

            rightSelected = defaultRightSelected;
        }

        public bool LeftSelected => !rightSelected;

        public bool RightSelected => rightSelected;

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

        public bool DefaultRightSelected { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSTwoButtonsSetting);

        public static implicit operator ASSTwoButtonsDisplay(SSTwoButtonsSetting twoButtons) => new(twoButtons.SettingId, twoButtons.Label, twoButtons.OptionA, twoButtons.OptionB, twoButtons.DefaultIsB, twoButtons.HintDescription, null, twoButtons.CollectionId);

        public static implicit operator SSTwoButtonsSetting(ASSTwoButtonsDisplay twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.LeftOption, twoButtons.RightOption, twoButtons.DefaultRightSelected, twoButtons.Hint, twoButtons.CollectionId);

        #if EXILED
        public static implicit operator ASSTwoButtonsDisplay(TwoButtonsSetting twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.FirstOption, twoButtons.SecondOption, twoButtons.IsSecondDefault, twoButtons.HintDescription, twoButtons.OnChanged.Convert(), twoButtons.CollectionId)
        {
            ExHeader = twoButtons.Header,
            ExAction = twoButtons.OnChanged,
        };

        public static implicit operator TwoButtonsSetting(ASSTwoButtonsDisplay twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.LeftOption, twoButtons.RightOption, twoButtons.DefaultRightSelected, twoButtons.Hint, twoButtons.CollectionId, false, twoButtons.ExHeader, twoButtons.ExAction);
        #endif

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
            writer.WriteBool(DefaultRightSelected);
        }

        internal override void Deserialize(NetworkReaderPooled reader)
        {
            rightSelected = reader.ReadBool();

            base.Deserialize(reader);
        }

        internal override ASSBase Copy() => new ASSTwoButtons(Id, Label, LeftOption, RightOption, DefaultRightSelected, Hint, OnChanged, CollectionId);

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