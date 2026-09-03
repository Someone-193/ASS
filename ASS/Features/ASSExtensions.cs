namespace ASS.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ASS.Features.Interfaces;
    using ASS.Features.Settings;
    using LabApi.Features.Wrappers;
    using NorthwoodLib.Pools;

    public static class ASSExtensions
    {
        public static List<ASSBase> GetAllSettings(this IASSObject assObject)
        {
            List<ASSBase> settings = [];
            List<IASSObject> previousObjects = ListPool<IASSObject>.Shared.Rent();
            InternalGetAllSettings(assObject, settings, previousObjects);
            ListPool<IASSObject>.Shared.Return(previousObjects);
            return settings;
        }

        public static List<ASSBase> GetViewableSettingsOrdered(this IASSObject assObject, Player? viewer)
        {
            if (viewer == null)
                return [];
            List<ASSBase> settings = [];
            List<IASSObject> previousGroups = ListPool<IASSObject>.Shared.Rent();
            InternalGetViewableSettingsOrdered(assObject, settings, previousGroups, viewer);
            ListPool<IASSObject>.Shared.Return(previousGroups);
            return settings;
        }

        private static void InternalGetAllSettings(IASSObject assObject, List<ASSBase> current, List<IASSObject> previousObjects)
        {
            if (assObject is ISettingsObject settings)
                current.AddRange(settings.Settings);

            if (assObject.Children == null)
                return;
            previousObjects.Add(assObject);
            foreach (IASSObject group in assObject.Children)
            {
                if (previousObjects.Contains(group))
                    throw new InvalidOperationException("ASS objects cannot reference themselves within their children.");
                InternalGetAllSettings(group, current, previousObjects);
            }

            previousObjects.Remove(assObject);
        }

        private static void InternalGetViewableSettingsOrdered(IASSObject assObject, List<ASSBase> current, List<IASSObject> previousChildren, Player viewer)
        {
            if (assObject is ISettingsObject settingsObject)
            {
                if (assObject.Viewers == null || assObject.Viewers(viewer))
                    current.AddRange(settingsObject.Settings);
            }

            if (assObject.Children == null)
                return;
            previousChildren.Add(assObject);
            foreach (IASSObject obj in assObject.Children.Where(obj => (obj.Viewers?.Invoke(viewer) ?? true)).OrderByDescending(group => group.Priority))
            {
                if (previousChildren.Contains(obj))
                    throw new InvalidOperationException("ASS objects cannot reference themselves within their children.");

                InternalGetViewableSettingsOrdered(obj, current, previousChildren, viewer);
            }

            previousChildren.Remove(assObject);
        }
    }
}