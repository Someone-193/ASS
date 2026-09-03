namespace ASS.Features.Interfaces
{
    using System.Collections.Generic;
    using ASS.Features.Settings;

    public interface ISettingsObject
    {
        public List<ASSBase> Settings { get; set; }
    }
}