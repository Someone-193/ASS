namespace ASS.Events.EventArgs
{
    using ASS.Events.Interfaces;
    using ASS.Features.Interfaces;

    public class ObjectRegisteredEventArgs : IASSEventArgs
    {
        public ObjectRegisteredEventArgs(IASSObject obj) => Object = obj;

        public IASSObject Object { get; }
    }
}