namespace ASS.Features.Instances.Displays
{
    using ASS.Features.Templates.Displays;

    using LabApi.Features.Wrappers;

    using Mirror;

    using UserSettings.ServerSpecific;

    public class DropdownDisplayInstance : ASSDisplayInstance
    {
        public DropdownDisplayInstance(Player viewer, int id, DropdownDisplayTemplate template)
            : base(viewer, id, template)
        {
            IndexSelected = template.IndexSelected;

            Options = template.Options;
            EntryType = template.EntryType;

            AutoSync = template.AutoSync;
        }

        public byte IndexSelected
        {
            get;
            set
            {
                field = value;
                UpdateSelection();
            }
        }

        public string OptionSelected => Options[IndexSelected];

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

            writer.WriteByte(IndexSelected);
            writer.WriteByte((byte)EntryType);
            writer.WriteByte((byte)Options.Length);

            Options.ForEach(writer.WriteString);
        }

        private void UpdateSelection()
            => UpdateDerived(writer =>
            {
                writer.WriteByte(1);
                writer.WriteByte(IndexSelected);
            });
    }
}