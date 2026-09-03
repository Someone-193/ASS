namespace ASS.Features.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ASS.Features.Interfaces;
    using ASS.Features.Settings;
    using LabApi.Features.Wrappers;
    using NorthwoodLib.Pools;

    public class ASSGroup : IASSObject, ISettingsObject
    {
        public ASSGroup(IEnumerable<ASSBase> settings, int priority = 0, Predicate<Player>? viewers = null, IEnumerable<IASSObject>? subGroups = null)
        {
            Settings = settings.ToList();
            Priority = priority;
            Viewers = viewers;
            Children = subGroups?.ToList();
        }

        public List<ASSBase> Settings { get; set; }

        public int Priority { get; set; }

        public Predicate<Player>? Viewers { get; set; }

        public List<IASSObject>? Children { get; set; }
    }
}