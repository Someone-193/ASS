namespace ASS.Features.Collections
{
    using System.Collections.Generic;

    using LabApi.Features.Wrappers;

    public abstract class AbstractMenu
    {
        public Dictionary<Player, ASSGroup> Groups { get; } = new();

        public void Add(Player player)
        {
            Groups[player] = Generate(player);

            ASSNetworking.RegisterObjects([Groups[player]], [player]);
        }

        public void Update(Player player, bool registerChange = true, bool ignoreDefaultResponses = false, bool onlyGroupsResponses = false)
        {
            if (!Groups.TryGetValue(player, out ASSGroup group))
                return;

            ASSGroup newGroup = Generate(player);

            group.Settings = newGroup.Settings;
            group.Priority = newGroup.Priority;
            group.Viewers = newGroup.Viewers;
            group.Children = newGroup.Children;

            ASSNetworking.SendToPlayerFull(player, true, registerChange, false, ignoreDefaultResponses || onlyGroupsResponses, onlyGroupsResponses ? group.GetAllSettings().ToArray() : null);
        }

        public void Remove(Player player)
        {
            if (!Groups.Remove(player, out ASSGroup group))
                return;

            ASSNetworking.UnregisterGroups([group], [player]);
        }

        protected abstract ASSGroup Generate(Player owner);
    }
}