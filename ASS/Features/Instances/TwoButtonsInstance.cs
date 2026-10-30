namespace ASS.Features.Instances
{
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Mirror;

    public class TwoButtonsInstance : ASSInstance
    {
        private bool rightSelected;

        public TwoButtonsInstance(Player viewer, int id, TwoButtonsTemplate template)
            : base(viewer, id, template)
        {
            AutoSync = template.AutoSync;
            OnChanged = template.OnChanged;

            DefaultRightSelected = template.DefaultRightSelected;
            LeftOption = template.LeftOption;
            RightOption = template.RightOption;
        }

        public InstanceHandler<TwoButtonsInstance>? OnChanged { get; set; }

        public bool LeftSelected => !rightSelected;

        public bool RightSelected => rightSelected;

        public bool DefaultRightSelected { get; set; }

        public string LeftOption
        {
            get;
            set
            {
                field = value;
                UpdateTwoButtons();
            }
        }

        public string RightOption
        {
            get;
            set
            {
                field = value;
                UpdateTwoButtons();
            }
        }

        public void UpdateTwoButtons()
        {
            if (!AutoSync)
                return;

            UpdateDerived(writer =>
            {
                writer.WriteByte(2);
                writer.WriteString(LeftOption);
                writer.WriteString(RightOption);
            });
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
    }
}