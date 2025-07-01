namespace ASS.Settings
{
    using System;
    using LabApi.Features.Wrappers;
    using Mirror;
    using UserSettings.ServerSpecific;

    public abstract class ASSBase
    {
        public int Id { get; set; }

        public string? Label { get; set; }

        public string? Hint { get; set; }

        public abstract Type SSSType { get; }

        public bool IgnoreNextResponse { get; set; }

        internal abstract ServerSpecificSettingBase.UserResponseMode ResponseMode { get; }

        public override string ToString()
        {
            return $"({Id}) {{{Label ?? "NULL"}}} [{Hint ?? "NULL"}] <{SSSType.Name}>";
        }

        internal virtual void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(Id);
            writer.WriteString(Label);
            writer.WriteString(Hint);
        }
    }
}