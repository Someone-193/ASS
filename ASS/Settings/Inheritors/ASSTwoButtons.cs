namespace ASS.Settings.Inheritors
{
    using System;
    using LabApi.Features.Wrappers;
    using Mirror;
    using UserSettings.ServerSpecific;

    public class ASSTwoButtons : ASSBase
    {
        public ASSTwoButtons(int id, string? label = null, string? leftOption = null, string? rightOption = null, bool defaultRightSelected = false, string? hint = null)
        {
            Id = id;
            Label = label;
            LeftOption = leftOption;
            RightOption = rightOption;
            DefaultRightSelected = defaultRightSelected;
            Hint = hint;

            RightSelected = defaultRightSelected;
        }

        public bool LeftSelected => !RightSelected;

        public bool RightSelected { get; private set; }

        public string? LeftOption { get; set; }

        public string? RightOption { get; set; }

        public bool DefaultRightSelected { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSTwoButtonsSetting);

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

        protected internal virtual void OnPressed(Player sender, ASSTwoButtons setting)
        {
        }
    }
}