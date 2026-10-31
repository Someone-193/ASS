namespace ASS.Features.Containers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using ASS.Features.Interfaces;

    using LabApi.Features.Wrappers;

    public class ASSGroup : IASSContainer
    {
        private readonly List<IASSObject> children = [];

        private ASSGroup(string key) => Key = key;

        public string Key { get; }

        public Predicate<Player>? Condition { get; set; }

        public ReadOnlyCollection<IASSObject> Children => new(children);

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public static ASSGroup Create(string key) => new(key);

        public ASSGroup AddChild(IASSObject child)
        {
            if (children.Contains(child) || !child.TrySetParent(this))
                return this;

            children.Add(child);
            return this;
        }

        public ASSGroup RemoveChild(IASSObject child)
        {
            if (children.Remove(child))
                child.TrySetParent(null);

            return this;
        }

        public ASSGroup ClearChildren()
        {
            foreach (IASSObject child in children)
                child.TrySetParent(null);

            children.Clear();
            return this;
        }

        public ASSGroup WithChildren(IEnumerable<IASSObject> newChildren)
        {
            foreach (IASSObject child in newChildren)
            {
                if (child.Parent is not null)
                    throw new InvalidOperationException("An object cannot be the child of two containers!");

                if (child.TrySetParent(this))
                    children.Add(child);
            }

            return this;
        }

        public ASSGroup WithCondition(Predicate<Player> predicate)
        {
            Condition = predicate;
            return this;
        }

        public ASSGroup Register(int priority, IEnumerable<Player>? toUpdate = null)
        {
            ASSManagement.RegisterObject(this, priority, toUpdate);
            return this;
        }

        public ASSGroup Unregister(IEnumerable<Player>? toUpdate = null)
        {
            ASSManagement.UnregisterObject(this, toUpdate);
            return this;
        }

        public IEnumerable<IASSObject> GetChildren(Player viewer) => Condition is null || Condition(viewer) ? children : [];
    }
}