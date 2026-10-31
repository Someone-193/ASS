namespace ASS.Features.Instances
{
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Mirror;

    using UnityEngine;

    using UserSettings.ServerSpecific;

    public class DropdownInstance : ASSInstance
    {
        private int indexSelected;
        private string optionSelected;

        public DropdownInstance(Player viewer, int id, DropdownTemplate template)
            : base(viewer, id, template)
        {
            OnDropdownChanged = template.OnDropdownChanged;

            Options = template.Options;
            DefaultIndex = template.DefaultIndex;
            EntryType = template.EntryType;

            indexSelected = DefaultIndex;
            optionSelected = Options[DefaultIndex];

            AutoSync = template.AutoSync;
        }

        public int IndexSelected => indexSelected;

        public string OptionSelected => optionSelected;

        public InstanceHandler<DropdownInstance>? OnDropdownChanged { get; set; }

        public string[] Options
        {
            get;
            set
            {
                if (value.Length is 0)
                    value = [string.Empty];

                field = value;
                UpdateDropdown();
            }
        }

        public byte DefaultIndex { get; set; }

        public SSDropdownSetting.DropdownEntryType EntryType { get; set; }

        public void UpdateDropdown()
        {
            if (!AutoSync)
                return;

            UpdateDerived(writer =>
            {
                writer.WriteByte(2);
                writer.WriteByte((byte)Options.Length);
                Options.ForEach(writer.WriteString);
            });
        }

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
    }
}