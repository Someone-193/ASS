namespace ASS.Features.Settings
{
    using System;
    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif
    using LabApi.Features.Wrappers;
    using Mirror;
    using UserSettings.ServerSpecific;

    public class ASSTwoButtons : ASSBase
    {
        private bool rightSelected;

        public ASSTwoButtons(
            int id,
            string? label = null,
            string? leftOption = null,
            string? rightOption = null,
            bool defaultRightSelected = false,
            string? hint = null,
            Action<Player, ASSBase>? onChanged = null,
            byte collectionId = byte.MaxValue)
        {
            Id = id;
            Label = label;
            LeftOption = leftOption;
            RightOption = rightOption;
            DefaultRightSelected = defaultRightSelected;
            Hint = hint;
            OnChanged = onChanged;

            rightSelected = defaultRightSelected;
        }

        public bool LeftSelected => !rightSelected;

        public bool RightSelected => rightSelected;

        public string? LeftOption { get; set; }

        public string? RightOption { get; set; }

        public bool DefaultRightSelected { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSTwoButtonsSetting);

        public static implicit operator ASSTwoButtons(SSTwoButtonsSetting twoButtons) => new(twoButtons.SettingId, twoButtons.Label, twoButtons.OptionA, twoButtons.OptionB, twoButtons.DefaultIsB, twoButtons.HintDescription, null, twoButtons.CollectionId);

        public static implicit operator SSTwoButtonsSetting(ASSTwoButtons twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.LeftOption, twoButtons.RightOption, twoButtons.DefaultRightSelected, twoButtons.Hint, twoButtons.CollectionId);

        #if EXILED
        public static implicit operator ASSTwoButtons(TwoButtonsSetting twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.FirstOption, twoButtons.SecondOption, twoButtons.IsSecondDefault, twoButtons.HintDescription, twoButtons.OnChanged.Convert(), twoButtons.CollectionId)
        {
            ExHeader = twoButtons.Header,
            ExAction = twoButtons.OnChanged,
        };

        public static implicit operator TwoButtonsSetting(ASSTwoButtons twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.LeftOption, twoButtons.RightOption, twoButtons.DefaultRightSelected, twoButtons.Hint, twoButtons.CollectionId, false, twoButtons.ExHeader, twoButtons.ExAction);
        #endif

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
    }
}