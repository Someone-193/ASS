namespace ASS.Features.MirrorUtils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using ASS.Events.EventArgs;
    using ASS.Events.Handlers;
    using ASS.Features.Instances;
    using ASS.Features.MirrorUtils.Messages;
    using ASS.Features.Templates;

    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;

    using Mirror;

    using NorthwoodLib.Pools;

    using UserSettings.ServerSpecific;

    internal static class ASSUtils
    {
        private static ArraySegment<byte> cache;

        public static bool TabOpen(this Player player)
        {
            return ServerSpecificSettingsSync.IsTabOpenForUser(player.ReferenceHub);
        }

        public static void SendASSMessage<T>(Player player, T message, int channelId = 0)
            where T : struct, NetworkMessage
        {
            if (player.IsDestroyed)
                return;

            using NetworkWriterPooled writer = NetworkWriterPool.Get();

            switch (message)
            {
                case ASSEntriesPack pack:
                    // Add PersistantKeybinds without overlapping keybinds.
                    HashSet<int> existing = HashSetPool<int>.Shared.Rent(pack.Instances.Select(setting => setting.Id));

                    bool flag = false;
                    foreach (KeybindTemplate keybind in ASSNetworking.PersistantKeybinds)
                    {
                        int id;
                        if (keybind.OverrideId.HasValue)
                            id = keybind.OverrideId.Value;
                        else if (keybind.Key is not null)
                            id = ASSExtensions.GenerateId(keybind.Key);
                        else
                        {
                            Logger.Error("Failed to send PersistantKeybinds.\nAll visible ASS Templates must have a usable ID when sending settings!");
                            continue;
                        }

                        if (!existing.Add(id))
                            continue;

                        if (!flag)
                        {
                            pack.Instances.Add(new HeaderInstance(player, "Plz No Hash Collision 2: Electric Boogaloo".GetStableHashCode(), HeaderTemplate.Create(string.Empty).WithLabel("Keybinds").WithHint("These are here so you can always use your keybinds, even if typically hidden")));
                            flag = true;
                        }

                        pack.Instances.Add(keybind.CreateInstance(player, id));
                    }

                    HashSetPool<int>.Shared.Return(existing);

                    // Call SendingSettings event
                    SendingInstancesEventArgs ev1 = new(player, pack.Instances);
                    SettingEvents.OnSendingSettings(ev1);
                    if (!ev1.IsAllowed)
                    {
                        ListPool<ASSInstance>.Shared.Return(pack.Instances);
                        return;
                    }

                    // remove old instances from their templates.
                    if (ASSNetworking.SentSettings.TryGetValue(player, out ASSInstance[]? oldInstances))
                    {
                        for (int i = 0; i < oldInstances.Length; i++)
                        {
                            oldInstances[i].Template.RemoveInstance(player);
                        }
                    }

                    // Save settings being sent
                    ASSNetworking.SentSettings[player] = pack.Instances.ToArray();

                    // Debugging
                    if (Main.Debug)
                    {
                        Logger.Debug("Sending Settings:");
                        foreach (ASSInstance setting in pack.Instances)
                        {
                            Logger.Debug(setting);
                        }
                    }

                    // Actually serialize
                    writer.WriteUShort(NetworkMessageId<SSSEntriesPack>.Id);
                    pack.Serialize(writer);
                    break;
                case ASSUpdateMessage update:
                    if (!ASSNetworking.SentSettings.TryGetValue(player, out ASSInstance[] settings))
                        return;

                    ASSInstance? toUpdate;
                    try
                    {
                        toUpdate = settings.Single(setting => setting.Id == update.Id);
                    }
                    catch (InvalidOperationException)
                    {
                        StackTrace trace = new();

                        Logger.Error("A method tried to update an ASS setting despite not existing in ASSNetworking.SentSettings!");
                        Logger.Error("Stack Trace:");
                        Logger.Error(trace);
                        return;
                    }

                    UpdatingInstanceEventArgs ev2 = new(player, toUpdate);
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
                Logger.Error($"NetworkConnection.Send: message of type {typeof(T)} with a size of {writer.Position} bytes is larger than the max allowed message size in one batch: {num}.\nThe message was dropped, please make it smaller.");
            }
            else
            {
                player.Connection.Send(writer.ToArraySegment(), channelId);
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

        public static void SendASSMessageToPlayers<T>(T message, IEnumerable<Player> players, int channelId = 0)
            where T : struct, NetworkMessage
        {
            using NetworkWriterPooled writer = NetworkWriterPool.Get();

            bool flag = false;
            foreach (Player player in players)
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

                player.Connection.Send(cache, channelId);
            }
        }
    }
}