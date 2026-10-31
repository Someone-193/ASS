namespace ASS.Features.Containers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ASS.Features.Interfaces;

    using LabApi.Features.Wrappers;

    public class PlayerMenu : IASSContainer
    {
        private PlayerMenu(string key, GroupUpdateHandler generator)
        {
            Key = key;
            Generator = generator;
        }

        public delegate IEnumerable<IASSObject> GroupUpdateHandler(Player player);

        public string Key { get; }

        public Predicate<Player>? Condition { get; set; }

        public GroupUpdateHandler Generator { get; set; }

        public Dictionary<Player, List<IASSObject>> Children { get; } = [];

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public static PlayerMenu Create(string key, GroupUpdateHandler generator) => new(key, generator);

        public bool TryAdd(Player viewer, bool updatePlayer = true, bool registerChange = true)
        {
            if (Children.ContainsKey(viewer))
                return false;

            Add(viewer, updatePlayer, registerChange);
            return true;
        }

        public void Add(Player viewer, bool updatePlayer = true, bool registerChange = true)
        {
            Children.Add(viewer, Generator(viewer).ToList());

            if (updatePlayer)
                ASSNetworking.SendToPlayerFull(viewer, registerChange: registerChange);
        }

        /// <summary>
        /// Updates a <see cref="PlayerMenu"/> by using the menus <see cref="GroupUpdateHandler"/> to acquire new values.
        /// </summary>
        /// <param name="viewer">The viewer to update.</param>
        /// <param name="updatePlayer">Whether to send the player the new settings.</param>
        /// <param name="registerChange">If true, makes a red dot appear next to the owners SSS tab indicating a change.</param>
        public void Update(Player viewer, bool updatePlayer = true, bool registerChange = true)
        {
            if (!Children.TryGetValue(viewer, out List<IASSObject>? children))
                return;

            children.Clear();
            foreach (IASSObject child in Generator(viewer))
            {
                if (child.Parent is not null)
                    throw new InvalidOperationException("An object cannot be the child of two containers!");

                if (child.TrySetParent(this))
                    children.Add(child);
            }

            if (updatePlayer)
                ASSNetworking.SendToPlayerFull(viewer, registerChange: registerChange);
        }

        public void UpdateAll(bool updatePlayer = true, bool registerChange = true)
        {
            foreach (KeyValuePair<Player, List<IASSObject>> kvp in Children)
            {
                kvp.Value.Clear();

                foreach (IASSObject child in Generator(kvp.Key))
                {
                    if (child.Parent is not null)
                        throw new InvalidOperationException("An object cannot be the child of two containers!");

                    if (child.TrySetParent(this))
                        kvp.Value.Add(child);
                }

                if (updatePlayer)
                    ASSNetworking.SendToPlayerFull(kvp.Key, registerChange: registerChange);
            }
        }

        public void Remove(Player viewer, bool updatePlayer = true)
        {
            bool removed = Children.Remove(viewer);

            if (updatePlayer && removed)
                ASSNetworking.SendToPlayer(viewer);
        }

        public PlayerMenu WithCondition(Predicate<Player> condition)
        {
            Condition = condition;
            return this;
        }

        public PlayerMenu Register(int priority, IEnumerable<Player>? toUpdate = null)
        {
            ASSManagement.RegisterObject(this, priority, toUpdate);
            return this;
        }

        public PlayerMenu Unregister(IEnumerable<Player>? toUpdate = null)
        {
            ASSManagement.UnregisterObject(this, toUpdate);
            return this;
        }

        public IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            if (Condition is not null && !Condition(viewer))
                return [];

            return Children.TryGetValue(viewer, out List<IASSObject> templates) ? templates : [];
        }
    }
}