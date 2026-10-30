namespace ASS.Features.Instances
{
    using System;

    using ASS.Features.MirrorUtils.Messages;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Mirror;

    using UserSettings.ServerSpecific;

    public abstract class ASSInstance
    {
        public ASSInstance(Player viewer, int id, ASSTemplate template)
        {
            Viewer = viewer;
            Template = template;

            Id = id;
            Key = template.Key;

            Label = template.Label;
            Hint = template.Hint;
            CollectionId = template.CollectionId;
        }

        public Player Viewer { get; }

        public int Id { get; }

        public string? Key { get; }

        public string? Label
        {
            get;
            set
            {
                field = value;
                Update();
            }
        }

        public string? Hint
        {
            get;
            set
            {
                field = value;
                Update();
            }
        }

        public byte CollectionId { get; set; }

        public bool AutoSync { get; set; }

        public ASSTemplate Template { get; }

        public bool IsFirstResponse => TimesTriggered == (ResponseMode is ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange ? 2 : 1);

        public bool InitialResponseReceived => ResponseMode is not ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange || TimesTriggered >= 1;

        public int TimesTriggered { get; internal set; }

        public ServerSpecificSettingBase.UserResponseMode ResponseMode => Template.ResponseMode;

        protected virtual bool IsServerOnly => false;

        public override string ToString() => $"({Id}) {{{Label ?? "NULL"}}} [{Hint ?? "NULL"}] <{Template.SSSType.Name}>";

        public void Update()
        {
            if (!AutoSync)
                return;

            MirrorUtils.ASSUtils.SendASSMessage(
                Viewer,
                new ASSUpdateMessage(Id, Template.SSSType, writer =>
                {
                    writer.WriteBool(true);
                    writer.WriteString(Label);
                    writer.WriteString(Hint);
                }));
        }

        internal virtual void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(Id);
            writer.WriteString(Label);
            writer.WriteString(Hint);
            writer.WriteByte(CollectionId);
            writer.WriteBool(IsServerOnly);
        }

        internal virtual void Deserialize(NetworkReaderPooled reader) => reader.Dispose();

        // for derived update methods
        protected void UpdateDerived(Action<NetworkWriter> action)
        {
            MirrorUtils.ASSUtils.SendASSMessage(
                Viewer,
                new ASSUpdateMessage(Id, Template.SSSType, writer =>
                {
                    writer.WriteBool(false);
                    action(writer);
                }));
        }
    }
}