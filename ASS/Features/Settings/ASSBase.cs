namespace ASS.Features.Settings
{
    using System;
    using Mirror;
    using UserSettings.ServerSpecific;

    public abstract class ASSBase
    {
        public int Id { get; set; }

        public string? Label { get; set; }

        public string? Hint { get; set; }

        public bool IgnoreNextResponse { get; set; }

        /// <summary>
        /// Gets the <see cref="ServerSpecificSettingBase.UserResponseMode"/> of the setting. Acquisition implies a response when the setting is received and Change implies a response whenever the setting is triggered.
        /// </summary>
        public abstract ServerSpecificSettingBase.UserResponseMode ResponseMode { get; }

        internal abstract Type SSSType { get; }

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

        internal virtual void Deserialize(NetworkReaderPooled reader)
        {
            reader.Dispose();
        }

        internal abstract ASSBase Copy();
    }
}