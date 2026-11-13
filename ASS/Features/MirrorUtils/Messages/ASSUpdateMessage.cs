namespace ASS.Features.MirrorUtils.Messages
{
    using System;
    using ASS.Features.Instances;
    using ASS.Features.Templates;
    using Mirror;
    using UserSettings.ServerSpecific;

    public readonly struct ASSUpdateMessage(int id, Type sssType, Action<NetworkWriter> writerFunc) : NetworkMessage
    {
        public readonly int Id = id;
        public readonly byte TypeCode = ServerSpecificSettingsSync.GetCodeFromType(sssType);
        public readonly Action<NetworkWriter> ServersidePayloadWriter = writerFunc;

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(Id);
            writer.WriteByte(TypeCode);
            using NetworkWriterPooled networkWriterPooled = NetworkWriterPool.Get();
            Action<NetworkWriter> serversidePayloadWriter = ServersidePayloadWriter;
            serversidePayloadWriter?.Invoke(networkWriterPooled);
            writer.WriteArraySegment(networkWriterPooled.ToArraySegment());
        }
    }
}