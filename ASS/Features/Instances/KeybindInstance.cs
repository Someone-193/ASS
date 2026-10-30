namespace ASS.Features.Instances
{
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Mirror;

    using UnityEngine;

    public class KeybindInstance : ASSInstance
    {
        private bool isPressed;

        public KeybindInstance(Player viewer, int id, KeybindTemplate template)
            : base(viewer, id, template)
        {
            OnChanged = template.OnChanged;

            SuggestedKeyCode = template.SuggestedKeyCode;
            TriggerInGUI = template.TriggerInGUI;
            TriggerInSpectator = template.TriggerInSpectator;
        }

        public bool IsPressed => isPressed;

        public InstanceHandler<KeybindInstance>? OnChanged { get; set; }

        public KeyCode SuggestedKeyCode { get; set; }

        public bool TriggerInGUI { get; set; }

        public bool TriggerInSpectator { get; set; }

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WriteBool(!TriggerInGUI);
            writer.WriteBool(TriggerInSpectator);
            writer.WriteInt((int)SuggestedKeyCode);
        }

        internal override void Deserialize(NetworkReaderPooled reader)
        {
            isPressed = reader.ReadBool();

            base.Deserialize(reader);
        }
    }
}