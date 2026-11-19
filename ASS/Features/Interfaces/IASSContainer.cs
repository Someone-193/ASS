namespace ASS.Features.Interfaces
{
    using System.Collections.Generic;
    using LabApi.Features.Wrappers;

    public interface IASSContainer : IASSObject
    {
        public IEnumerable<IASSObject> GetChildren(Player viewer);
    }
}