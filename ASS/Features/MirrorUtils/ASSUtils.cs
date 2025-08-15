namespace ASS.Features.MirrorUtils
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using ASS.Events.EventArgs;
    using ASS.Events.Handlers;
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
            Player? player = Player.Get(connection.identity);
            if (player is null)
            {
                Logger.Error("Failed to find player from NetworkConnection in ASSUtils.SendASSMessage<T>();");
                return;
            }

            using NetworkWriterPooled writer = NetworkWriterPool.Get();

            switch (message)
            {
                case ASSEntriesPack pack:
                    foreach (ASSBase setting in pack.Settings.ToArray())
                    {
                        SendingSettingEventArgs ev1 = new(player, setting);
                        SettingEvents.OnSendingSetting(ev1);
                        if (!ev1.IsAllowed)
                            pack.Settings.Remove(ev1.Setting);
                    }

                    ASSNetworking.ReceivedSettings[player] = pack.Settings.ToArray();

                    if (Main.Debug)
                    {
                        Logger.Debug("Sending Settings:");
                        foreach (ASSBase setting in pack.Settings)
                        {
                            Logger.Debug(setting);
                        }
                    }

                    writer.WriteUShort(NetworkMessageId<SSSEntriesPack>.Id);
                    pack.Serialize(writer);
                    break;
                case ASSUpdateMessage update:
                    if (!ASSNetworking.ReceivedSettings.TryGetValue(player, out ASSBase[] settings))
                        return;

                    ASSBase? toUpdate;
                    try
                    {
                        toUpdate = settings.Single(setting => setting.Id == update.Id);
                    }
                    catch (InvalidOperationException)
                    {
                        StackTrace trace = new();

                        Logger.Error("A method tried to update an ASS setting despite not existing in ASSNetworking.ReceivedSettings!");
                        Logger.Error("Stack Trace:");
                        Logger.Error(trace);
                        return;
                    }

                    UpdatingSettingEventArgs ev2 = new(player, toUpdate);
                    SettingEvents.OnUpdatingSetting(ev2);
                    if (!ev2.IsAllowed)
                        return;

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