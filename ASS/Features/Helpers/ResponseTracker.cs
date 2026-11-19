namespace ASS.Features.Helpers
{
    using System;
    using System.Collections.Generic;

    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;

    using UserSettings.ServerSpecific;

    /// <summary>
    /// Tracks expected responses per player/setting so that duplicate responses (from
    /// multiple packs containing the same setting) collapse into a single trigger.
    /// </summary>
    internal static class ResponseTracker
    {
        private const float GraceFloat = 1F;

        private static readonly TimeSpan Grace = TimeSpan.FromSeconds(GraceFloat);

        private static readonly List<int> RemoveNonAlloc = [];

        private static Dictionary<Player, Dictionary<int, Entry>> Entries { get; } = new();

        internal static void Expect(Player player, int id, DateTime now)
        {
            if (!Entries.TryGetValue(player, out Dictionary<int, Entry>? dict))
                Entries[player] = dict = new Dictionary<int, Entry>();

            if (!dict.TryGetValue(id, out Entry? entry))
                dict[id] = entry = new Entry();

            entry.Remaining++;
            entry.Deadline = now + Grace;
        }

        internal static void OnResponse(Player player, SSSClientResponse response)
        {
            if (!Entries.TryGetValue(player, out Dictionary<int, Entry>? dict) || !dict.TryGetValue(response.Id, out Entry? entry))
            {
                ASSNetworking.HandleResponse(player, response);
                return;
            }

            entry.Cached = response;
            entry.HasCached = true;
            entry.Remaining--;

            if (entry.Remaining > 0)
            {
                Logger.Debug($"Duplicate response detected for setting with ID '{response.Id}'!", Main.Debug);
                return;
            }

            dict.Remove(response.Id);

            ASSNetworking.HandleResponse(player, response);
        }

        internal static void Remove(Player player) => Entries.Remove(player);

        internal static void Flush(DateTime now)
        {
            foreach ((Player player, Dictionary<int, Entry> dict) in Entries)
            {
                RemoveNonAlloc.Clear();

                foreach ((int id, Entry entry) in dict)
                {
                    if (now < entry.Deadline)
                        continue;

                    if (entry.HasCached)
                        ASSNetworking.HandleResponse(player, entry.Cached!.Value);

                    RemoveNonAlloc.Add(id);
                }

                for (int i = 0; i < RemoveNonAlloc.Count; i++)
                    dict.Remove(RemoveNonAlloc[i]);
            }
        }

        private class Entry
        {
            public int Remaining { get; set; }

            public DateTime Deadline { get; set; }

            public SSSClientResponse? Cached { get; set; }

            public bool HasCached { get; set; }
        }
    }
}