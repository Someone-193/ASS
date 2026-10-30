namespace ASS.Features.Instances
{
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Mirror;

    public class ButtonInstance : ASSInstance
    {
        public ButtonInstance(Player viewer, int id, ButtonTemplate template)
            : base(viewer, id, template)
        {
            AutoSync = template.AutoSync;
            OnPressed = template.OnPressed;

            ButtonText = template.ButtonText;
            HoldTime = template.HoldTime;
        }

        public InstanceHandler<ButtonInstance>? OnPressed { get; set; }

        public string ButtonText
        {
            get;
            set
            {
                field = value;
                UpdateButton();
            }
        }

        public float HoldTime
        {
            get;
            set
            {
                field = value;
                UpdateButton();
            }
        }

        public void UpdateButton()
        {
            if (!AutoSync)
                return;

            UpdateDerived(writer =>
            {
                writer.WriteString(ButtonText);
                writer.WriteFloat(HoldTime);
            });
        }

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteFloat(HoldTime);
            writer.WriteString(ButtonText);
        }
    }
}