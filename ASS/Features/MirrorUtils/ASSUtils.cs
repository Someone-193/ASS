namespace ASS.Features.MirrorUtils
{
    using System;
    using System.Linq;

    using ASS.Features.MirrorUtils.Messages;
    using ASS.Features.Settings;

    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;

    using Mirror;

    using UserSettings.ServerSpecific;

    internal static class ASSUtils
    {
        private static ArraySegment<byte> cache;

        public static bool TabOpen(this Player player)
        {
            return ServerSpecificSettingsSync.IsTabOpenForUser(player.ReferenceHub);
        }

        public static void SendASSMessage<T>(NetworkConnection connection, T message, int channelId = 0) 
            where T : struct, NetworkMessage
        {
            using NetworkWriterPooled writer = NetworkWriterPool.Get();
            switch (message)
            {
                case ASSEntriesPack pack:
                    Player? player = Player.Get(connection.identity);
                    if (player is not null)
                    {
                        foreach (ASSBase setting in pack.Settings ?? [])
                        {
                            // C# kinda annoying. And I don't want to copy paste Exileds events rn.
                            ASSNetworking.Bridge(player, setting);
                        }
                    }

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

        public static void SendASSMessageToAuthenticated<T>(T message, int channelId = 0)
            where T : struct, NetworkMessage
        {
            SendASSMessageToPlayersConditionally(message, plyr => plyr.ReferenceHub.Mode != 0, channelId);
        }

        public static void SendASSMessageToPlayersConditionally<T>(T message, Predicate<Player> predicate, int channelId = 0)
            where T : struct, NetworkMessage
        {
            if (!NetworkServer.active)
                throw new InvalidOperationException("Can not use SendToHubsConditionally because NetworkServer is not active!");

            using NetworkWriterPooled writer = NetworkWriterPool.Get();

            bool flag = false;
            foreach (ReferenceHub hub in ReferenceHub.AllHubs.Where(hub => predicate(Player.Get(hub))))
            {
                if (!flag)
                {
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

                    cache = writer.ToArraySegment();
                    flag = true;
                }

                hub.connectionToClient.Send(cache, channelId);
            }
        }
    }
}