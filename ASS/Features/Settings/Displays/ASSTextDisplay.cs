namespace ASS.Features.Settings.Displays
{
    using System;

    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif

    using Mirror;

    using TMPro;

    using UserSettings.ServerSpecific;

    public class ASSTextDisplay : ASSDisplay
    {
        public ASSTextDisplay(
            int id,
            string? content = null,
            string? collapsedText = null,
            SSTextArea.FoldoutMode foldoutMode = SSTextArea.FoldoutMode.NotCollapsable,
            TextAlignmentOptions alignmentOptions = TextAlignmentOptions.TopLeft)
        {
            Id = id;
            Label = content;
            Hint = collapsedText;
            FoldoutMode = foldoutMode;
            AlignmentOptions = alignmentOptions;
        }

        public SSTextArea.FoldoutMode FoldoutMode { get; set; }

        public TextAlignmentOptions AlignmentOptions { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.None;

        internal override Type SSSType { get; } = typeof(SSTextArea);

        public static implicit operator ASSTextDisplay(SSTextArea textArea) => new(textArea.SettingId, textArea.Label, textArea.HintDescription, textArea.Foldout, textArea.AlignmentOptions);

        public static implicit operator SSTextArea(ASSTextDisplay textArea) => new(textArea.Id, textArea.Label, textArea.FoldoutMode, textArea.Hint, textArea.AlignmentOptions);

        #if EXILED
        public static implicit operator ASSTextDisplay(TextInputSetting textArea) => new(textArea.Id, textArea.Label, textArea.HintDescription, textArea.FoldoutMode, textArea.Alignment)
        {
            ExHeader = textArea.Header,
        };

        public static implicit operator TextInputSetting(ASSTextDisplay textArea) => new(textArea.Id, textArea.Label, textArea.FoldoutMode, textArea.AlignmentOptions, textArea.Hint, textArea.ExHeader, textArea.ExAction);
        #endif

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteByte((byte)FoldoutMode);
            writer.WriteInt((int)AlignmentOptions);
        }

        internal override ASSBase Copy() => new ASSTextDisplay(Id, Label, Hint, FoldoutMode, AlignmentOptions);
    }
}