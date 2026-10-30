namespace ASS.Features.Instances.Displays
{
    using ASS.Features.Templates.Displays;

    using LabApi.Features.Wrappers;

    using Mirror;

    public class TwoButtonsDisplayInstance : ASSDisplayInstance
    {
        public TwoButtonsDisplayInstance(Player viewer, int id, TwoButtonsDisplayTemplate template)
            : base(viewer, id, template)
        {
            RightSelected = template.RightSelected;

            LeftOption = template.LeftOption;
            RightOption = template.RightOption;

            AutoSync = template.AutoSync;
        }

        public bool LeftSelected
        {
            get => !RightSelected;
            set => RightSelected = !value;
        }

        public bool RightSelected
        {
            get;
            set
            {
                field = value;
                UpdateValue();
            }
        }

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
            writer.WriteBool(RightSelected);
        }

        private void UpdateValue()
            => UpdateDerived(
                writer =>
                {
                    writer.WriteByte(1);
                    writer.WriteBool(RightSelected);
                });
    }
}