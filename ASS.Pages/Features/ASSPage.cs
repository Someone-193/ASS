namespace ASS.Pages.Features
{
    using System;
    using System.Collections.Generic;
    using ASS.Features.Interfaces;
    using LabApi.Features.Wrappers;

    public class ASSPage : IASSObject
    {
        public List<IASSObject>? Children { get; set; }

        public Predicate<Player>? Viewers { get; set; }

        public int Priority { get; set; }
    }
}