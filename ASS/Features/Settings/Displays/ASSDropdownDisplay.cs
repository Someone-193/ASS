namespace ASS.Features.Settings.Displays
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif

    using LabApi.Features.Wrappers;
    using Mirror;
    using UnityEngine;
    using UserSettings.ServerSpecific;
    using Logger = LabApi.Features.Console.Logger;

    public class ASSDropdownDisplay : ASSDisplay
    {
        public ASSDropdownDisplay(
            int id,
            string? label = null,
            string[]? options = null,
            byte indexSelected = 0,
            SSDropdownSetting.DropdownEntryType entryType = SSDropdownSetting.DropdownEntryType.Regular,
            string? hint = null)
        {
            if (options is null || options.Length == 0)
            {
                options = [string.Empty];
            }

            if (indexSelected >= options.Length)
            {
                Logger.Warn($"Default index out of range in dropdown setting ctor with Id {id}. Clamping to valid value");
                indexSelected = (byte)Mathf.Min(Mathf.Clamp(indexSelected, 0, options.Length - 1), 255);
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
            IndexSelected = indexSelected;
            EntryType = entryType;
            Hint = hint;

            IndexSelected = indexSelected;
            OptionSelected = options[indexSelected];
        }

        public byte IndexSelected { get; set; }

        public string OptionSelected { get; set; }

        public string[] Options { get; set; }

        public SSDropdownSetting.DropdownEntryType EntryType { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        public override bool IsServerOnly => true;

        internal override Type SSSType { get; } = typeof(SSDropdownSetting);

        public static implicit operator ASSDropdownDisplay(SSDropdownSetting dropdown) => new(dropdown.SettingId, dropdown.Label, dropdown.Options, (byte)dropdown.DefaultOptionIndex, dropdown.EntryType, dropdown.HintDescription);

        public static implicit operator SSDropdownSetting(ASSDropdownDisplay dropdown) => new(dropdown.Id, dropdown.Label, dropdown.Options, dropdown.IndexSelected, dropdown.EntryType, dropdown.Hint, dropdown.CollectionId);

        #if EXILED
        public static implicit operator ASSDropdownDisplay(DropdownSetting dropdown) => new(dropdown.Id, dropdown.Label, dropdown.Options.ToArray(), (byte)dropdown.DefaultOptionIndex, dropdown.DropdownType, dropdown.HintDescription)
        {
            ExHeader = dropdown.Header,
            ExAction = dropdown.OnChanged,
        };

        public static implicit operator DropdownSetting(ASSDropdownDisplay dropdown) => new(dropdown.Id, dropdown.Label, dropdown.Options, dropdown.IndexSelected, dropdown.EntryType, dropdown.Hint, dropdown.CollectionId, false, dropdown.ExHeader, dropdown.ExAction);
        #endif

        public void UpdateSelection(byte selection, IEnumerable<Player>? players)
        {
            UpdateDerived(
                writer =>
                {
                    writer.WriteByte(1);
                    writer.WriteByte(selection);
                },
                players);
        }

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteByte(IndexSelected);
            writer.WriteByte((byte)EntryType);
            writer.WriteByte((byte)Options.Length);

            Options.ForEach(writer.WriteString);
        }

        internal override ASSBase Copy() => new ASSDropdown(Id, Label, Options, IndexSelected, EntryType, Hint, OnChanged, CollectionId);
    }
}