namespace ASS.Features.Instances.Displays
{
    using ASS.Features.Templates.Displays;

    using LabApi.Features.Wrappers;

    using Mirror;

    using TMPro;

    using UserSettings.ServerSpecific;

    public class TextDisplayInstance : ASSDisplayInstance
    {
        public TextDisplayInstance(Player viewer, int id, TextDisplayTemplate template)
            : base(viewer, id, template)
        {
            AutoSync = template.AutoSync;

            FoldoutMode = template.FoldoutMode;
            AlignmentOptions = template.AlignmentOptions;
        }

        public SSTextArea.FoldoutMode FoldoutMode { get; set; }

        public TextAlignmentOptions AlignmentOptions { get; set; }

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteByte((byte)FoldoutMode);
            writer.WriteInt((int)AlignmentOptions);
        }
    }
}