namespace ASS.Settings.Inheritors
{
    using System;
    using LabApi.Features.Wrappers;
    using Mirror;
    using UnityEngine;
    using UserSettings.ServerSpecific;

    using Logger = LabApi.Features.Console.Logger;

    public class ASSDropdown : ASSBase
    {
        public ASSDropdown(int id, string? label = null, string[]? options = null, byte defaultIndex = 0, SSDropdownSetting.DropdownEntryType entryType = SSDropdownSetting.DropdownEntryType.Regular, string? hint = null)
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

            OptionSelected = options[defaultIndex];
        }

        public int IndexSelected { get; private set; }

        public string OptionSelected { get; private set; }

        public string[] Options { get; set; }

        public byte DefaultIndex { get; set; }

        public SSDropdownSetting.DropdownEntryType EntryType { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSDropdownSetting);

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
            IndexSelected = Mathf.Clamp(reader.ReadByte(), 0, Options.Length - 1);
            OptionSelected = Options[IndexSelected];

            base.Deserialize(reader);
        }

        internal override ASSBase Copy() => new ASSDropdown(Id, Label, Options, DefaultIndex, EntryType, Hint);

        protected internal virtual void OnTriggered(Player sender, ASSDropdown dropdown)
        {
        }
    }
}