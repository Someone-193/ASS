namespace ASS.Settings.Inheritors
{
    using System;
    using LabApi.Features.Wrappers;
    using Mirror;
    using UnityEngine;
    using UserSettings.ServerSpecific;

    public class ASSKeybind : ASSBase
    {
        public ASSKeybind(int id, string? label = null, KeyCode suggestedKeyCode = KeyCode.None, bool triggerInGUI = false, bool triggerInSpectator = false, string? hint = null)
        {
            Id = id;
            Label = label;
            SuggestedKeyCode = suggestedKeyCode;
            TriggerInGUI = triggerInGUI;
            TriggerInSpectator = triggerInSpectator;
            Hint = hint;
        }

        public bool IsPressed { get; private set; }

        public KeyCode SuggestedKeyCode { get; set; }

        public bool TriggerInGUI { get; set; }

        public bool TriggerInSpectator { get; set; }

        internal override Type SSSType { get; } = typeof(SSKeybindSetting);

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteBool(TriggerInGUI);
            writer.WriteBool(TriggerInSpectator);
            writer.WriteInt((int)SuggestedKeyCode);
        }

        internal override void Deserialize(NetworkReaderPooled reader)
        {
            IsPressed = reader.ReadBool();

            base.Deserialize(reader);
        }

        internal override ASSBase Copy() => new ASSKeybind(Id, Label, SuggestedKeyCode, TriggerInGUI, TriggerInSpectator, Hint);

        protected internal virtual void OnPressed(Player player, ASSKeybind keybind)
        {
        }
    }
}