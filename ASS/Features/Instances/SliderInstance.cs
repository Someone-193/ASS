namespace ASS.Features.Instances
{
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Mirror;

    public class SliderInstance : ASSInstance
    {
        private float value;
        private bool dragging;

        public SliderInstance(Player viewer, int id, SliderTemplate template)
            : base(viewer, id, template)
        {
            OnChanged = template.OnChanged;

            DefaultValue = template.DefaultValue;
            MinValue = template.MinValue;
            MaxValue = template.MaxValue;
            IsInteger = template.IsInteger;
            ValueFormat = template.ValueFormat;
            DisplayFormat = template.DisplayFormat;

            AutoSync = template.AutoSync;
        }

        public float Value => value;

        public int IntValue => (int)value;

        public bool Dragging => dragging;

        public InstanceHandler<SliderInstance>? OnChanged { get; set; }

        public float DefaultValue { get; set; }

        public float MinValue
        {
            get;
            set
            {
                field = value;
                UpdateSlider();
            }
        }

        public float MaxValue
        {
            get;
            set
            {
                field = value;
                UpdateSlider();
            }
        }

        public bool IsInteger
        {
            get;
            set
            {
                field = value;
                UpdateSlider();
            }
        }

        public string ValueFormat
        {
            get;
            set
            {
                field = value;
                UpdateSlider();
            }
        }

        public string DisplayFormat
        {
            get;
            set
            {
                field = value;
                UpdateSlider();
            }
        }

        public void UpdateSlider()
        {
            if (!AutoSync)
                return;

            UpdateDerived(writer =>
            {
                writer.WriteByte(2);
                writer.WriteFloat(MinValue);
                writer.WriteFloat(MaxValue);
                writer.WriteBool(IsInteger);
                writer.WriteString(ValueFormat);
                writer.WriteString(DisplayFormat);
            });
        }

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteFloat(DefaultValue);
            writer.WriteFloat(MinValue);
            writer.WriteFloat(MaxValue);
            writer.WriteBool(IsInteger);
            writer.WriteString(ValueFormat);
            writer.WriteString(DisplayFormat);
        }

        internal override void Deserialize(NetworkReaderPooled reader)
        {
            value = reader.ReadFloat();
            dragging = reader.ReadBool();

            base.Deserialize(reader);
        }
    }
}