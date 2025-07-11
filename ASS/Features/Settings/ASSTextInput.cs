namespace ASS.Features.Settings
{
    using System;
    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif
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
            string? hint = null,
            Action<Player, ASSBase>? onChanged = null)
        {
            Id = id;
            Label = label;
            Placeholder = placeholder;
            CharacterLimit = characterLimit;
            ContentType = contentType;
            Hint = hint;
            OnChanged = onChanged;
        }

        public string InputtedText { get; private set; } = string.Empty;

        public string Placeholder { get; set; }

        public int CharacterLimit { get; set; }

        public TMP_InputField.ContentType ContentType { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSPlaintextSetting);

        public static implicit operator ASSTextInput(SSPlaintextSetting text) => new(text.SettingId, text.Label, text.Placeholder, text.CharacterLimit, text.ContentType, text.HintDescription);

        public static implicit operator SSPlaintextSetting(ASSTextInput text) => new(text.Id, text.Label, text.Placeholder, text.CharacterLimit, text.ContentType, text.Hint);

        #if EXILED
        public static implicit operator ASSTextInput(UserTextInputSetting text) => new(text.Id, text.Label, text.PlaceHolder, text.CharacterLimit, text.ContentType, text.HintDescription);

        public static implicit operator UserTextInputSetting(ASSTextInput text) => new(text.Id, text.Label, text.Placeholder, text.CharacterLimit, text.ContentType, text.Hint);
        #endif

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