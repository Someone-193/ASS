namespace ASS.Features.Templates.Displays
{
    using System;

    using TMPro;

    using UserSettings.ServerSpecific;

    public partial class TextDisplayTemplate : ASSDisplayTemplate
    {
        public SSTextArea.FoldoutMode FoldoutMode { get; set; }

        public TextAlignmentOptions AlignmentOptions { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.None;

        internal override Type SSSType { get; } = typeof(SSTextArea);
    }
}