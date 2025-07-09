namespace ASS.Settings.Inheritors
{
    using System;
    using ASS.MirrorUtils;
    using ASS.MirrorUtils.Messages;
    using LabApi.Features.Wrappers;
    using Mirror;
    using TMPro;
    using UserSettings.ServerSpecific;

    public class ASSTextDisplay : ASSBase
    {
        public ASSTextDisplay(int id, string? content, string? collapsedText, SSTextArea.FoldoutMode foldoutMode, TextAlignmentOptions alignmentOptions)
        {
            Id = id;
            Label = content;
            Hint = collapsedText;
            FoldoutMode = foldoutMode;
            AlignmentOptions = alignmentOptions;
        }

        public SSTextArea.FoldoutMode FoldoutMode { get; set; }

        public TextAlignmentOptions AlignmentOptions { get; set; }

        internal override Type SSSType { get; } = typeof(SSTextArea);

        public void SendTextUpdate(string newText, bool applyOverride = true, Predicate<Player>? filter = null)
        {
            if (applyOverride)
                Label = newText;
            ASSUpdateMessage assUpdateMessage = new(this, writer => writer.WriteString(newText));
            if (filter == null)
                ASSUtils.SendASSMessageToAuthenticated(assUpdateMessage);
            else
                ASSUtils.SendASSMessageToPlayersConditionally(assUpdateMessage, filter);
        }

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteByte((byte)FoldoutMode);
            writer.WriteInt((int)AlignmentOptions);
        }

        internal override ASSBase Copy() => new ASSTextDisplay(Id, Label, Hint, FoldoutMode, AlignmentOptions);
    }
}