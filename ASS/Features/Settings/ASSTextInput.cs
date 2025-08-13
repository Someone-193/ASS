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
        private string inputtedText = string.Empty;

        public ASSTextInput(
            int id,
            string? label = null,
            string placeholder = "...",
            int characterLimit = 64,
            TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard,
            string? hint = null,
            Action<Player, ASSBase>? onChanged = null,
            byte collectionId = byte.MaxValue)
        {
            Id = id;
            Label = label;
            Placeholder = placeholder;
            CharacterLimit = characterLimit;
            ContentType = contentType;
            Hint = hint;
            OnChanged = onChanged;
            CollectionId = collectionId;
        }

        public string InputtedText => inputtedText;

        public string Placeholder { get; set; }

        public int CharacterLimit { get; set; }

        public TMP_InputField.ContentType ContentType { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSPlaintextSetting);

        public static implicit operator ASSTextInput(SSPlaintextSetting text) => new(text.SettingId, text.Label, text.Placeholder, text.CharacterLimit, text.ContentType, text.HintDescription);

        public static implicit operator SSPlaintextSetting(ASSTextInput text) => new(text.Id, text.Label, text.Placeholder, text.CharacterLimit, text.ContentType, text.Hint);

        #if EXILED
        public static implicit operator ASSTextInput(UserTextInputSetting text) => new(text.Id, text.Label, text.PlaceHolder, text.CharacterLimit, text.ContentType, text.HintDescription, text.OnChanged.Convert(), text.CollectionId)
        {
            ExHeader = text.Header,
            ExAction = text.OnChanged,
        };

        public static implicit operator UserTextInputSetting(ASSTextInput text) => new(text.Id, text.Label, text.Placeholder, text.CharacterLimit, text.ContentType, text.Hint, text.CollectionId, false, text.ExHeader, text.ExAction);
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
            inputtedText = reader.ReadString();

            base.Deserialize(reader);
        }

        internal override ASSBase Copy() => new ASSTextInput(Id, Label, Placeholder, CharacterLimit, ContentType, Hint, OnChanged, CollectionId);
    }
}