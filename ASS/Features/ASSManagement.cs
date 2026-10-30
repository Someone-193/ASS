namespace ASS.Features
{
    using System.Collections.Generic;

    using ASS.Events.EventArgs;
    using ASS.Events.Handlers;
    using ASS.Features.Interfaces;

    using LabApi.Features.Wrappers;

    public static class ASSManagement
    {
        public static void RegisterObject(IASSObject obj, int priority, IEnumerable<Player>? toUpdate = null)
        {
            ASSNetworking.ObjectsValue.Add(obj, priority);
            SettingEvents.ObjectRegistered.Invoke(new ObjectRegisteredEventArgs(obj));

            if (toUpdate is null)
                return;

            foreach (Player player in toUpdate)
                ASSNetworking.SendToPlayer(player);
        }

        public static void UnregisterObject(IASSObject obj, IEnumerable<Player>? toUpdate = null)
        {
            ASSNetworking.ObjectsValue.Remove(obj);
            SettingEvents.ObjectUnregistered.Invoke(new ObjectUnregisteredEventArgs(obj));

            if (toUpdate is null)
                return;

            foreach (Player player in toUpdate)
                ASSNetworking.SendToPlayer(player);
        }

        public static void UnregisterGroups(IEnumerable<IASSObject> objects, IEnumerable<Player>? toUpdate = null)
        {
            foreach (IASSObject obj in objects)
            {
                ASSNetworking.ObjectsValue.Remove(obj);
                SettingEvents.ObjectUnregistered.Invoke(new ObjectUnregisteredEventArgs(obj));
            }

            if (toUpdate is null)
                return;

            foreach (Player player in toUpdate)
                ASSNetworking.SendToPlayer(player);
        }
    }
}