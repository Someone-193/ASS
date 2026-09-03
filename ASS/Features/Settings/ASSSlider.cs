namespace ASS.Features.Settings
{
    using System;
    using System.Collections.Generic;

    #if EXILED
    using Exiled.API.Features.Core.UserSettings;

    using Player = Exiled.API.Features.Player;
    #endif

    using LabApi.Features.Wrappers;

    using Mirror;

    using UserSettings.ServerSpecific;

    public class ASSSlider : ASSBase
    {
        private float value;

        private bool dragging;
        private float minValue;
        private float maxValue;
        private bool isInteger;
        private string valueFormat;
        private string displayFormat;

        public ASSSlider(
            int id,
            string? label,
            float defaultValue = 0,
            float minValue = -10,
            float maxValue = 10,
            bool isInteger = false,
            string valueFormat = "0.##",
            string displayFormat = "{0}",
            string? hint = null,
            Action<Player, ASSBase>? onChanged = null,
            byte collectionId = byte.MaxValue)
        {
            Id = id;
            Label = label;
            DefaultValue = defaultValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.isInteger = isInteger;
            this.valueFormat = valueFormat;
            this.displayFormat = displayFormat;
            Hint = hint;
            OnChanged = onChanged;
            CollectionId = collectionId;
        }

        public float Value => value;

        public bool Dragging => dragging;

        public float DefaultValue { get; set; }

        public float MinValue
        {
            get => minValue;
            set
            {
                minValue = value;
                if (AutoSync && IsInstance)
                    UpdateSlider(this.SettingHolders());
            }
        }

        public float MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;
                if (AutoSync && IsInstance)
                    UpdateSlider(this.SettingHolders());
            }
        }

        public bool IsInteger
        {
            get => isInteger;
            set
            {
                isInteger = value;
                if (AutoSync && IsInstance)
                    UpdateSlider(this.SettingHolders());
            }
        }

        public string ValueFormat
        {
            get => valueFormat;
            set
            {
                valueFormat = value;
                if (AutoSync && IsInstance)
                    UpdateSlider(this.SettingHolders());
            }
        }

        public string DisplayFormat
        {
            get => displayFormat;
            set
            {
                displayFormat = value;
                if (AutoSync && IsInstance)
                    UpdateSlider(this.SettingHolders());
            }
        }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSSliderSetting);

        public static implicit operator ASSSlider(SSSliderSetting slider) => new(slider.SettingId, slider.Label, slider.DefaultValue, slider.MinValue, slider.MaxValue, slider.Integer, slider.ValueToStringFormat, slider.FinalDisplayFormat, slider.HintDescription, null, slider.CollectionId);

        public static implicit operator SSSliderSetting(ASSSlider slider) => new(slider.Id, slider.Label, slider.DefaultValue, slider.MinValue, slider.MaxValue, slider.IsInteger, slider.ValueFormat, slider.DisplayFormat, slider.Hint, slider.CollectionId);

        #if EXILED
        public static implicit operator ASSSlider(SliderSetting slider) => new(slider.Id, slider.Label, slider.DefaultValue, slider.MinimumValue, slider.MaximumValue, slider.IsInteger, slider.StringFormat, slider.DisplayFormat, slider.HintDescription, slider.OnChanged.Convert(), slider.CollectionId)
        {
            ExHeader = slider.Header,
            ExAction = slider.OnChanged,
        };

        public static implicit operator SliderSetting(ASSSlider slider) => new(slider.Id, slider.Label, slider.DefaultValue, slider.MinValue, slider.MaxValue, slider.IsInteger, slider.ValueFormat, slider.DisplayFormat, slider.Hint, slider.CollectionId, false, slider.ExHeader, slider.ExAction);
        #endif

        public void UpdateMin(float newMinValue, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(newMinValue, MaxValue, IsInteger, ValueFormat, DisplayFormat), players);
        }

        public void UpdateMax(float newMaxValue, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(MinValue, newMaxValue, IsInteger, ValueFormat, DisplayFormat), players);
        }

        public void UpdateIsInteger(bool newIsInteger, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(MinValue, MaxValue, newIsInteger, ValueFormat, DisplayFormat), players);
        }

        public void UpdateValueFormat(string newValueFormat, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(MinValue, MaxValue, IsInteger, newValueFormat, DisplayFormat), players);
        }

        public void UpdateDisplayFormat(string newDisplayFormat, IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(MinValue, MaxValue, IsInteger, ValueFormat, newDisplayFormat), players);
        }

        public void UpdateSlider(IEnumerable<Player>? players)
        {
            UpdateDerived(GetAction(MinValue, MaxValue, IsInteger, ValueFormat, DisplayFormat), players);
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

        internal override ASSBase Copy() => new ASSSlider(Id, Label, DefaultValue, MinValue, MaxValue, IsInteger, ValueFormat, DisplayFormat, Hint, OnChanged, CollectionId);

        private static Action<NetworkWriter> GetAction(float newMinValue, float newMaxValue, bool newIsInteger, string newValueFormat, string newDisplayFormat)
        {
            return writer =>
            {
                writer.WriteByte(2);
                writer.WriteFloat(newMinValue);
                writer.WriteFloat(newMaxValue);
                writer.WriteBool(newIsInteger);
                writer.WriteString(newValueFormat);
                writer.WriteString(newDisplayFormat);
            };
        }
    }
}