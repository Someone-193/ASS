namespace ASS.Features.Settings
{
    using System;
    using System.Linq;
    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif
    using LabApi.Features.Wrappers;
    using Mirror;
    using UnityEngine;
    using UserSettings.ServerSpecific;
    using Logger = LabApi.Features.Console.Logger;

    public class ASSDropdown : ASSBase
    {
        private int indexSelected;

        private string optionSelected;

        public ASSDropdown(
            int id,
            string? label = null,
            string[]? options = null,
            byte defaultIndex = 0,
            SSDropdownSetting.DropdownEntryType entryType = SSDropdownSetting.DropdownEntryType.Regular,
            string? hint = null,
            Action<Player, ASSBase>? onChanged = null,
            byte collectionId = byte.MaxValue)
        {
            if (options is null || options.Length == 0)
            {
                options = [string.Empty];
            }

            if (defaultIndex >= options.Length)
            {
                Logger.Warn($"Default index out of range in dropdown setting ctor with Id {id}. Clamping to valid value");
                defaultIndex = (byte)Mathf.Min(Mathf.Clamp(defaultIndex, 0, options.Length - 1), 255);
            }

            if (options.Length >= byte.MaxValue)
            {
                Logger.Warn($"Option count out of range in dropdown setting ctor with Id {id}. Clamping to valid value");
                string[] temp = new string[byte.MaxValue];

                for (int i = 0; i < byte.MaxValue; i++)
                {
                    temp[i] = options[i];
                }

                options = temp;
            }

            Id = id;
            Label = label;
            Options = options;
            DefaultIndex = defaultIndex;
            EntryType = entryType;
            Hint = hint;
            OnChanged = onChanged;
            CollectionId = collectionId;

            optionSelected = options[defaultIndex];
        }

        public int IndexSelected => indexSelected;

        public string OptionSelected => optionSelected;

        public string[] Options { get; set; }

        public byte DefaultIndex { get; set; }

        public SSDropdownSetting.DropdownEntryType EntryType { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSDropdownSetting);

        public static implicit operator ASSDropdown(SSDropdownSetting dropdown) => new(dropdown.SettingId, dropdown.Label, dropdown.Options, (byte)dropdown.DefaultOptionIndex, dropdown.EntryType, dropdown.HintDescription, null, dropdown.CollectionId);

        public static implicit operator SSDropdownSetting(ASSDropdown dropdown) => new(dropdown.Id, dropdown.Label, dropdown.Options, dropdown.DefaultIndex, dropdown.EntryType, dropdown.Hint, dropdown.CollectionId);

        #if EXILED
        public static implicit operator ASSDropdown(DropdownSetting dropdown) => new(dropdown.Id, dropdown.Label, dropdown.Options.ToArray(), (byte)dropdown.DefaultOptionIndex, dropdown.DropdownType, dropdown.HintDescription, dropdown.OnChanged.Convert(), dropdown.CollectionId)
        {
            ExHeader = dropdown.Header,
            ExAction = dropdown.OnChanged,
        };

        public static implicit operator DropdownSetting(ASSDropdown dropdown) => new(dropdown.Id, dropdown.Label, dropdown.Options, dropdown.DefaultIndex, dropdown.EntryType, dropdown.Hint, dropdown.CollectionId, false, dropdown.ExHeader, dropdown.ExAction);
        #endif

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteByte(DefaultIndex);
            writer.WriteByte((byte)EntryType);
            writer.WriteByte((byte)Options.Length);

            Options.ForEach(writer.WriteString);
        }

        internal override void Deserialize(NetworkReaderPooled reader)
        {
            indexSelected = Mathf.Clamp(reader.ReadByte(), 0, Options.Length - 1);
            optionSelected = Options[IndexSelected];

            base.Deserialize(reader);
        }

        internal override ASSBase Copy() => new ASSDropdown(Id, Label, Options, DefaultIndex, EntryType, Hint, OnChanged, CollectionId);
    }
}