namespace ASS.Settings
{
    using System;
    using ASS.Settings.Inheritors;
    using Mirror;

    public abstract class ASSBase
    {
        public int Id { get; set; }

        public string? Label { get; set; }

        public string? Hint { get; set; }

        public bool IgnoreNextResponse { get; set; }

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