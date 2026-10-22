namespace ASS.Features.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ASS.Features.Settings;

    using LabApi.Features.Wrappers;

    using NorthwoodLib.Pools;

    public class ASSGroup
    {
        public ASSGroup(IEnumerable<ASSBase> settings, int priority = 0, Predicate<Player>? viewers = null, IEnumerable<ASSGroup>? subGroups = null)
        {
            Settings = settings.ToList();
            Priority = priority;
            Viewers = viewers;
            SubGroups = subGroups?.ToList();
        }

        public List<ASSBase> Settings { get; set; }

        public int Priority { get; set; }

        public Predicate<Player>? Viewers { get; set; }

        public List<ASSGroup>? SubGroups { get; set; }

        public List<ASSBase> GetAllSettings()
        {
            List<ASSBase> settings = [];
            List<ASSGroup> previousGroups = ListPool<ASSGroup>.Shared.Rent();
            InternalGetAllSettings(settings, previousGroups);
            ListPool<ASSGroup>.Shared.Return(previousGroups);
            return settings;
        }

        public List<ASSBase> GetViewableSettingsOrdered(Player? viewer)
        {
            if (viewer == null)
                return [];
            List<ASSBase> settings = [];
            List<ASSGroup> previousGroups = ListPool<ASSGroup>.Shared.Rent();
            InternalGetViewableSettingsOrdered(settings, previousGroups, viewer);
            ListPool<ASSGroup>.Shared.Return(previousGroups);
            return settings;
        }

        private void InternalGetAllSettings(List<ASSBase> current, List<ASSGroup> previousGroups)
        {
            current.AddRange(Settings);
            if (SubGroups == null)
                return;
            previousGroups.Add(this);
            foreach (ASSGroup group in SubGroups)
            {
                if (previousGroups.Contains(group))
                    throw new InvalidOperationException("ASSGroups cannot reference themselves within subgroups.");
                group.InternalGetAllSettings(current, previousGroups);
            }

            previousGroups.Remove(this);
        }

        private void InternalGetViewableSettingsOrdered(List<ASSBase> current, List<ASSGroup> previousGroups, Player viewer)
        {
            if (Viewers == null || Viewers(viewer))
                current.AddRange(Settings);
            if (SubGroups == null)
                return;
            previousGroups.Add(this);
            foreach (ASSGroup group in SubGroups.Where(group => group.Viewers == null || group.Viewers(viewer)).OrderByDescending(group => group.Priority))
            {
                if (previousGroups.Contains(group))
                    throw new InvalidOperationException("ASSGroups cannot reference themselves within subgroups.");
                group.InternalGetViewableSettingsOrdered(current, previousGroups, viewer);
            }

            previousGroups.Remove(this);
        }
    }
}