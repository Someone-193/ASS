namespace ASS.Features.Settings
{
    using System;
    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif
    using LabApi.Features.Wrappers;
    using Mirror;
    using UnityEngine;
    using UserSettings.ServerSpecific;

    public class ASSKeybind : ASSBase
    {
        public ASSKeybind(
            int id,
            string? label = null,
            KeyCode suggestedKeyCode = KeyCode.None,
            bool triggerInGUI = false,
            bool triggerInSpectator = false,
            string? hint = null,
            Action<Player, ASSBase>? onChanged = null)
        {
            Id = id;
            Label = label;
            SuggestedKeyCode = suggestedKeyCode;
            TriggerInGUI = triggerInGUI;
            TriggerInSpectator = triggerInSpectator;
            Hint = hint;
            OnChanged = onChanged;
        }

        public bool IsPressed { get; private set; }

        public KeyCode SuggestedKeyCode { get; set; }

        public bool TriggerInGUI { get; set; }

        public bool TriggerInSpectator { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange;

        internal override Type SSSType { get; } = typeof(SSKeybindSetting);

        public static implicit operator ASSKeybind(SSKeybindSetting keybind) => new(keybind.SettingId, keybind.Label, keybind.SuggestedKey, keybind.PreventInteractionOnGUI, keybind.AllowSpectatorTrigger, keybind.HintDescription);

        public static implicit operator SSKeybindSetting(ASSKeybind keybind) => new(keybind.Id, keybind.Label, keybind.SuggestedKeyCode, keybind.TriggerInGUI, keybind.TriggerInSpectator, keybind.Hint);

        #if EXILED
        public static implicit operator ASSKeybind(KeybindSetting keybind) => new(keybind.Id, keybind.Label, keybind.KeyCode, keybind.PreventInteractionOnGUI, keybind.AllowSpectatorTrigger, keybind.HintDescription, keybind.OnChanged.Convert())
        {
            ExHeader = keybind.Header,
            ExAction = keybind.OnChanged,
        };

        public static implicit operator KeybindSetting(ASSKeybind keybind) => new(keybind.Id, keybind.Label, keybind.SuggestedKeyCode, !keybind.TriggerInGUI, keybind.TriggerInSpectator, keybind.Hint, keybind.ExHeader, keybind.ExAction);
        #endif

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteBool(!TriggerInGUI);
            writer.WriteBool(TriggerInSpectator);
            writer.WriteInt((int)SuggestedKeyCode);
        }

        internal override void Deserialize(NetworkReaderPooled reader)
        {
            IsPressed = reader.ReadBool();

            base.Deserialize(reader);
        }

        internal override ASSBase Copy() => new ASSKeybind(Id, Label, SuggestedKeyCode, TriggerInGUI, TriggerInSpectator, Hint);
    }
}