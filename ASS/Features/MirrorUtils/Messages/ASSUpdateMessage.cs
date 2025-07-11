namespace ASS.Features.MirrorUtils.Messages
{
    using System;
    using ASS.Features.Settings;
    using Mirror;
    using UserSettings.ServerSpecific;

    public readonly struct ASSUpdateMessage(ASSBase setting, Action<NetworkWriter> writerFunc) : NetworkMessage
    {
        public readonly int Id = setting.Id;
        public readonly byte TypeCode = ServerSpecificSettingsSync.GetCodeFromType(setting.SSSType);
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