namespace ASS.Features.Templates
{
    using System;

    using ASS.Features.Instances;

    using UnityEngine;

    using UserSettings.ServerSpecific;

    public partial class KeybindTemplate : ASSTemplate
    {
        public InstanceHandler<KeybindInstance>? OnChanged { get; set; }

        public KeyCode SuggestedKeyCode { get; set; } = KeyCode.None;

        public bool TriggerInGUI { get; set; }

        public bool TriggerInSpectator { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSKeybindSetting);
    }
}