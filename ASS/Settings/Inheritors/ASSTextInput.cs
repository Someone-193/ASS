namespace ASS.Settings.Inheritors
{
    using System;
    using LabApi.Features.Wrappers;
    using Mirror;
    using TMPro;
    using UserSettings.ServerSpecific;

    public class ASSTextInput : ASSBase
    {
        public ASSTextInput(
            int id,
            string? label = null,
            string placeholder = "...",
            int characterLimit = 64,
            TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard,
            string? hint = null)
        {
            Id = id;
            Label = label;
            Placeholder = placeholder;
            CharacterLimit = characterLimit;
            ContentType = contentType;
            Hint = hint;
        }

        public string InputtedText { get; private set; } = string.Empty;

        public string Placeholder { get; set; }

        public int CharacterLimit { get; set; }

        public TMP_InputField.ContentType ContentType { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSPlaintextSetting);

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteString(Placeholder);
            writer.WriteUShort((ushort)CharacterLimit);
            writer.WriteByte((byte)ContentType);
        }

        internal override void Deserialize(NetworkReaderPooled reader)
        {
            InputtedText = reader.ReadString();

            base.Deserialize(reader);
        }

        internal override ASSBase Copy() => new ASSTextInput(Id, Label, Placeholder, CharacterLimit, ContentType, Hint);
    }
}