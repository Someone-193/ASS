namespace ASS.Settings.Inheritors
{
    using System;
    using LabApi.Features.Wrappers;
    using Mirror;
    using UserSettings.ServerSpecific;

    public class ASSSlider : ASSBase
    {
        public ASSSlider(
            int id,
            string? label,
            float defaultValue = 0,
            float minValue = -10,
            float maxValue = 10,
            bool isInteger = false,
            string valueFormat = "0.##",
            string displayFormat = "{0}",
            string? hint = null)
        {
            Id = id;
            Label = label;
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
            IsInteger = isInteger;
            ValueFormat = valueFormat;
            DisplayFormat = displayFormat;
            Hint = hint;
        }

        public float Value { get; private set; }

        public bool Dragging { get; private set; }

        public float DefaultValue { get; set; }

        public float MinValue { get; set; }

        public float MaxValue { get; set; }

        public bool IsInteger { get; set; }

        public string ValueFormat { get; set; }

        public string DisplayFormat { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSSliderSetting);

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
            Value = reader.ReadFloat();
            Dragging = reader.ReadBool();

            base.Deserialize(reader);
        }

        internal override ASSBase Copy() => new ASSSlider(Id, Label, DefaultValue, MinValue, MaxValue, IsInteger, ValueFormat, DisplayFormat, Hint);

        protected internal virtual void OnMoved(Player player, ASSSlider slider)
        {
        }
    }
}