namespace ASS.MirrorUtils
{
    using ASS.MirrorUtils.Messages;
    using LabApi.Features.Console;
    using Mirror;
    using UserSettings.ServerSpecific;

    internal class ASSUtils
    {
        // private ArraySegment<byte> cache;
        public static void SendASSMessage<T>(NetworkConnection connection, T message, int channelId = 0) 
            where T : struct, NetworkMessage
        {
            using NetworkWriterPooled writer = NetworkWriterPool.Get();
            switch (message)
            {
                case ASSEntriesPack pack:
                    writer.WriteUShort(NetworkMessageId<SSSEntriesPack>.Id);
                    pack.Serialize(writer);
                    break;
                case ASSUpdateMessage update:
                    writer.WriteUShort(NetworkMessageId<SSSUpdateMessage>.Id);
                    update.Serialize(writer);
                    break;
            }

            int num = NetworkMessages.MaxMessageSize(channelId);
            if (writer.Position > num)
            {
                Logger.Error($"NetworkConnection.Send: message of type {(object)typeof(T)} with a size of {(object)writer.Position} bytes is larger than the max allowed message size in one batch: {(object)num}.\nThe message was dropped, please make it smaller.");
            }
            else
            {
                connection.Send(writer.ToArraySegment(), channelId);
            }
        }
    }
}