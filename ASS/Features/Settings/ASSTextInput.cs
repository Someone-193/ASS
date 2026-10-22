namespace ASS.Features.Settings
{
    using System;
    using System.Collections.Generic;

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

        private string placeholder;
        private ushort characterLimit;
        private TMP_InputField.ContentType contentType;

        public ASSTextInput(
            int id,
            string? label = null,
            string defaultText = "",
            string placeholder = "...",
            ushort characterLimit = 64,
            TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard,
            string? hint = null,
            Action<Player, ASSBase>? onChanged = null,
            byte collectionId = byte.MaxValue)
        {
            Id = id;
            Label = label;
            DefaultText = defaultText;
            this.placeholder = placeholder;
            this.characterLimit = characterLimit;
            this.contentType = contentType;
            Hint = hint;
            OnChanged = onChanged;
            CollectionId = collectionId;
        }

        public string InputtedText => inputtedText;

        /// <summary>
        /// Gets or sets the default value of this <see cref="ASSTextInput"/>.
        /// </summary>
        /// <remarks>
        /// Cannot automatically sync cuz NW moment.
        /// </remarks>
        public string DefaultText { get; set; }

        public string Placeholder
        {
            get => placeholder;
            set
            {
                placeholder = value;
                if (AutoSync && IsInstance)
                    UpdateTextInput(this.SettingHolders());
            }
        }

        public ushort CharacterLimit
        {
            get => characterLimit;
            set
            {
                characterLimit = value;
                if (AutoSync && IsInstance)
                    UpdateTextInput(this.SettingHolders());
            }
        }

        public TMP_InputField.ContentType ContentType
        {
            get => contentType;
            set
            {
                contentType = value;
                if (AutoSync && IsInstance)
                    UpdateTextInput(this.SettingHolders());
            }
        }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSPlaintextSetting);

        public static implicit operator ASSTextInput(SSPlaintextSetting text) => new(text.SettingId, text.Label, text.DefaultText, text.Placeholder, (ushort)text.CharacterLimit, text.ContentType, text.HintDescription);

        public static implicit operator SSPlaintextSetting(ASSTextInput text) => new(text.Id, text.Label, text.Placeholder, text.CharacterLimit, text.ContentType, text.Hint)
        {
            DefaultText = text.DefaultText,
        };

        #if EXILED
        public static implicit operator ASSTextInput(UserTextInputSetting text) => new(text.Id, text.Label, text.PlaceHolder, string.Empty, (ushort)text.CharacterLimit, text.ContentType, text.HintDescription, text.OnChanged.Convert(), text.CollectionId)
        {
            ExHeader = text.Header,
            ExAction = text.OnChanged,
        };

        public static implicit operator UserTextInputSetting(ASSTextInput text) => new(text.Id, text.Label, text.Placeholder, text.CharacterLimit, text.ContentType, text.Hint, text.CollectionId, false, text.ExHeader, text.ExAction);
        #endif

        /// <summary>
        /// Requests all players in <see cref="players"/> to clear their TextInput.
        /// </summary>
        /// <param name="players">The players to receive this request. If null, defaults to all setting holders.</param>
        public void RequestClear(IEnumerable<Player>? players)
        {
            UpdateDerived(writer => writer.WriteByte(1), players);
        }

        public void UpdatePlaceHolder(string newPlaceHolder, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(newPlaceHolder, CharacterLimit, ContentType), players);
        }

        public void UpdateCharacterLimit(ushort newCharacterLimit, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(Placeholder, newCharacterLimit, ContentType), players);
        }

        public void UpdateContentType(TMP_InputField.ContentType newContentType, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(Placeholder, CharacterLimit, newContentType), players);
        }

        public void UpdateTextInput(IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(Placeholder, CharacterLimit, ContentType), players);
        }

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteString(DefaultText);
            writer.WriteString(Placeholder);
            writer.WriteUShort(CharacterLimit);
            writer.WriteByte((byte)ContentType);
        }

        internal override void Deserialize(NetworkReaderPooled reader)
        {
            inputtedText = reader.ReadString();

            base.Deserialize(reader);
        }

        internal override ASSBase Copy() => new ASSTextInput(Id, Label, DefaultText, Placeholder, CharacterLimit, ContentType, Hint, OnChanged, CollectionId);

        private static Action<NetworkWriter> GetAction(string newPlaceHolder, ushort newCharacterLimit, TMP_InputField.ContentType newContentType)
        {
            return writer =>
            {
                writer.WriteByte(3);
                writer.WriteString(newPlaceHolder);
                writer.WriteUShort(newCharacterLimit);
                writer.WriteByte((byte)newContentType);
            };
        }
    }
}