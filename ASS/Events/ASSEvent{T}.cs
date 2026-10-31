namespace ASS.Events
{
    using System;
    using System.Collections.Generic;
    using LabApi.Features.Console;

    public delegate void ASSEventHandler<in T>(T ev);

    // This is based off Exiled's Event{T}.
    public class ASSEvent<T>
    {
        private record Registration
        {
            public Registration(ASSEventHandler<T> handler, int priority)
            {
                Handler = handler;
                Priority = priority;
            }

            public ASSEventHandler<T> Handler { get; init; }

            public int Priority { get; init; }
        }

        private static readonly IComparer<Registration> Comparer = Comparer<Registration>.Create((x, y) => y.Priority - x.Priority);

        private readonly List<Registration> delegates = [];

        public static ASSEvent<T> operator +(ASSEvent<T> assEvent, ASSEventHandler<T> handler)
        {
            assEvent.Subscribe(handler);
            return assEvent;
        }

        public static ASSEvent<T> operator -(ASSEvent<T> assEvent, ASSEventHandler<T> handler)
        {
            assEvent.Unsubscribe(handler);
            return assEvent;
        }

        public void Invoke(T ev)
        {
            for (int i = 0; i < delegates.Count; i++)
            {
                try
                {
                    delegates[i].Handler(ev);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Method \"{delegates[i].Handler.Method.Name}\" of the class \"{delegates[i].Handler.Method.ReflectedType?.FullName ?? "Null"}\" caused an exception when handling the event \"{GetType().FullName}\"\n{ex}");
                }
            }
        }

        public void Subscribe(ASSEventHandler<T> handler) => Subscribe(handler, 0);

        public void Subscribe(ASSEventHandler<T> handler, int priority)
        {
            Registration registration = new(handler, priority);
            int index = delegates.BinarySearch(registration, Comparer);
            if (index < 0)
            {
                delegates.Insert(~index, registration);
            }
            else
            {
                while (index < delegates.Count && delegates[index].Priority == priority)
                    index++;
                delegates.Insert(index, registration);
            }
        }

        public void Unsubscribe(ASSEventHandler<T> handler)
        {
            int index = delegates.FindIndex(p => p.Handler == handler);
            if (index != -1)
                delegates.RemoveAt(index);
        }
    }
}