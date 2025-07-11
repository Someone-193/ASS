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
        public ASSTwoButtons(
            int id,
            string? label = null,
            string? leftOption = null,
            string? rightOption = null,
            bool defaultRightSelected = false,
            string? hint = null,
            Action<Player, ASSBase>? onChanged = null)
        {
            Id = id;
            Label = label;
            LeftOption = leftOption;
            RightOption = rightOption;
            DefaultRightSelected = defaultRightSelected;
            Hint = hint;
            OnChanged = onChanged;

            RightSelected = defaultRightSelected;
        }

        public bool LeftSelected => !RightSelected;

        public bool RightSelected { get; private set; }

        public string? LeftOption { get; set; }

        public string? RightOption { get; set; }

        public bool DefaultRightSelected { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSTwoButtonsSetting);

        #if EXILED
        public static implicit operator ASSTwoButtons(SSTwoButtonsSetting twoButtons) => new(twoButtons.SettingId, twoButtons.Label, twoButtons.OptionA, twoButtons.OptionB, twoButtons.DefaultIsB, twoButtons.HintDescription);

        public static implicit operator SSTwoButtonsSetting(ASSTwoButtons twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.LeftOption, twoButtons.RightOption, twoButtons.DefaultRightSelected, twoButtons.Hint);

        public static implicit operator ASSTwoButtons(TwoButtonsSetting twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.FirstOption, twoButtons.SecondOption, twoButtons.IsSecondDefault, twoButtons.HintDescription, twoButtons.OnChanged.Convert())
        {
            ExHeader = twoButtons.Header,
            ExAction = twoButtons.OnChanged,
        };

        public static implicit operator TwoButtonsSetting(ASSTwoButtons twoButtons) => new(twoButtons.Id, twoButtons.Label, twoButtons.LeftOption, twoButtons.RightOption, twoButtons.DefaultRightSelected, twoButtons.Hint, twoButtons.ExHeader, twoButtons.ExAction);
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
            RightSelected = reader.ReadBool();

            base.Deserialize(reader);
        }

        internal override ASSBase Copy() => new ASSTwoButtons(Id, Label, LeftOption, RightOption, DefaultRightSelected, Hint);
    }
}