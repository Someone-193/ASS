namespace ASS.Events.EventArgs
{
    using ASS.Events.Interfaces;
    using ASS.Features.Interfaces;

    public class ObjectUnregisteredEventArgs : IASSEventArgs
    {
        public ObjectUnregisteredEventArgs(IASSObject obj) => Object = obj;

        public IASSObject Object { get; }
    }
}