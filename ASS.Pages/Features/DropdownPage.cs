namespace ASS.Pages.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ASS.Features;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;
    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;
    using UserSettings.ServerSpecific;

    public class DropdownPage : ASSPage
    {
        private DropdownPage(string key)
            : base(key)
        {
            Dropdown = DropdownTemplate.Create(ControlKey).WithHandler(Handler);
        }

        private DropdownTemplate Dropdown { get; }

        private Dictionary<string, IASSObject> Children { get; } = new();

        private Dictionary<Player, string> Selected { get; } = new();

        private string Default => Dropdown.Options[Dropdown.DefaultIndex];

        public static DropdownPage Create(string key) => new(key);

        public DropdownPage WithLabel(string label)
        {
            Dropdown.WithLabel(label);
            return this;
        }

        public DropdownPage WithHint(string hint)
        {
            Dropdown.WithHint(hint);
            return this;
        }

        public DropdownPage WithCollectionId(byte collectionId)
        {
            Dropdown.WithCollectionId(collectionId);
            return this;
        }

        public DropdownPage WithDefaultIndex(byte defaultIndex)
        {
            Dropdown.WithDefaultIndex(defaultIndex);
            return this;
        }

        public DropdownPage WithEntryType(SSDropdownSetting.DropdownEntryType entryType)
        {
            Dropdown.WithEntryType(entryType);
            return this;
        }

        public DropdownPage AddPage(string name, IASSObject child)
        {
            if (child.Key is ControlKey)
                throw new ArgumentException("You know what you did.");

            if (Children.ContainsKey(name))
            {
                Logger.Error($"Duplicate page name for {Key}! {name}");
                return this;
            }

            if (child.Parent is not null)
                throw new InvalidOperationException("An object cannot be the child of two containers!");

            if (!child.TrySetParent(this))
                return this;

            Children[name] = child;

            Dropdown.Options = Children.Keys.ToArray();
            foreach (Player viewer in Selected.Keys)
                ASSNetworking.SendToPlayer(viewer);

            Selected.Clear();
            return this;
        }

        public DropdownPage RemovePage(string name)
        {
            Children.Remove(name);
            Dropdown.Options = Children.Keys.ToArray();
            foreach (Player viewer in Selected.Keys)
                ASSNetworking.SendToPlayer(viewer);

            Selected.Clear();
            return this;
        }

        public string GetSelected(Player viewer) => Selected.TryGetValue(viewer, out string? value) ? value : string.Empty;

        public override IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            if (Children.Count is 0)
                yield break;

            yield return Dropdown;

            if (!Selected.TryGetValue(viewer, out string? selected)
                || !Children.TryGetValue(selected, out IASSObject? child))
                yield break;

            yield return child;
        }

        private void Handler(Player player, DropdownInstance dropdown)
        {
            if (dropdown.OptionSelected == Default)
            {
                if (Opened.Remove(player))
                {
                    if (SelectedPage.TryGetValue(player, out Stack<ASSPage> stack) && stack.TryPeek(out ASSPage? page) && page == this)
                        stack.Pop();
                }
            }
            else
            {
                if (Opened.Add(player))
                {
                    if (SelectedPage.TryGetValue(player, out Stack<ASSPage> stack))
                        stack.Push(this);
                }
            }

            Selected[player] = dropdown.OptionSelected;
        }
    }
}