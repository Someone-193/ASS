namespace ASS.Features.Instances.Displays
{
    using ASS.Features.Templates.Displays;

    using LabApi.Features.Wrappers;

    using Mirror;

    public class SliderDisplayInstance : ASSDisplayInstance
    {
        public SliderDisplayInstance(Player viewer, int id, SliderDisplayTemplate template)
            : base(viewer, id, template)
        {
            AutoSync = template.AutoSync;

            Value = template.Value;

            MinValue = template.MinValue;
            MaxValue = template.MaxValue;
            IsInteger = template.IsInteger;
            ValueFormat = template.ValueFormat;
            DisplayFormat = template.DisplayFormat;
        }

        public float Value
        {
            get;
            set
            {
                field = value;
                UpdateValue();
            }
        }

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

            writer.WriteFloat(Value);
            writer.WriteFloat(MinValue);
            writer.WriteFloat(MaxValue);
            writer.WriteBool(IsInteger);
            writer.WriteString(ValueFormat);
            writer.WriteString(DisplayFormat);
        }

        private void UpdateValue()
            => UpdateDerived(writer =>
            {
                writer.WriteByte(1);
                writer.WriteFloat(Value);
            });
    }
}