namespace ASS.Features.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ASS.Features.Settings;
    using LabApi.Features.Wrappers;
    using NorthwoodLib.Pools;

    public interface IASSObject
    {
        public List<IASSObject>? Children { get; set; }

        public Predicate<Player>? Viewers { get; set; }

        public int Priority { get; set; }
    }
}