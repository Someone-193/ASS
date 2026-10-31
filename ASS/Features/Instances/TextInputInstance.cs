namespace ASS.Features.Instances
{
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Mirror;

    using TMPro;

    public class TextInputInstance : ASSInstance
    {
        private string inputtedText = string.Empty;

        public TextInputInstance(Player viewer, int id, TextInputTemplate template)
            : base(viewer, id, template)
        {
            OnTextInputted = template.OnTextInputted;

            DefaultText = template.DefaultText;
            Placeholder = template.Placeholder;
            CharacterLimit = template.CharacterLimit;

            AutoSync = template.AutoSync;
        }

        public InstanceHandler<TextInputInstance>? OnTextInputted { get; set; }

        public string InputtedText => inputtedText;

        public string DefaultText { get; set; }

        public string Placeholder
        {
            get;
            set
            {
                field = value;
                UpdateTextInput();
            }
        }

        public ushort CharacterLimit
        {
            get;
            set
            {
                field = value;
                UpdateTextInput();
            }
        }

        public TMP_InputField.ContentType ContentType
        {
            get;
            set
            {
                field = value;
                UpdateTextInput();
            }
        }

        /// <summary>
        /// Requests the owner of this instance to clear their TextInput.
        /// </summary>
        public void RequestClear() => UpdateDerived(writer => writer.WriteByte(1));

        public void UpdateTextInput()
        {
            if (!AutoSync)
                return;

            UpdateDerived(writer =>
            {
                writer.WriteByte(3);
                writer.WriteString(Placeholder);
                writer.WriteUShort(CharacterLimit);
                writer.WriteByte((byte)ContentType);
            });
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
    }
}