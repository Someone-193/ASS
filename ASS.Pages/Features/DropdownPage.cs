namespace ASS.Pages.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ASS.Features;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Handlers;
    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;

    using UserSettings.ServerSpecific;

    // this is a really freaking complicated class due to the functionality it needs.
    public class DropdownPage : ASSPage, IDisposable
    {
        private DropdownPage(string key, string defaultPage, IASSObject defaultChild)
            : base(key)
        {
            Default = defaultPage;

            Children[defaultPage] = DropdownChild.Create(defaultChild);

            PlayerEvents.Left += OnLeft;
        }

        public Dictionary<Player, DropdownTemplate> GeneratedDropdowns { get; } = new();

        public Dictionary<string, DropdownChild> Children { get; } = new();

        public Dictionary<Player, string> Selected { get; } = new();

        private string Default { get; }

        private string Label { get; set; } = string.Empty;

        private string Hint { get; set; } = string.Empty;

        private byte CollectionId { get; set; }

        private SSDropdownSetting.DropdownEntryType EntryType { get; set; }

        public static DropdownPage Create(string key, string defaultPage, IASSObject defaultChild) => new(key, defaultPage, defaultChild);

        public DropdownPage WithLabel(string label)
        {
            Label = label;
            SetForeachDropdown(i => i.Label = label);
            return this;
        }

        public DropdownPage WithHint(string hint)
        {
            Hint = hint;
            SetForeachDropdown(i => i.Hint = hint);
            return this;
        }

        public DropdownPage WithCollectionId(byte collectionId)
        {
            CollectionId = collectionId;
            SetForeachDropdown(i => i.CollectionId = collectionId);
            return this;
        }

        public DropdownPage WithEntryType(SSDropdownSetting.DropdownEntryType entryType)
        {
            EntryType = entryType;
            SetForeachDropdown(i => i.EntryType = entryType);
            return this;
        }

        public DropdownPage AddPage(string name, IASSObject child) => AddPage(name, DropdownChild.Create(child));

        public DropdownPage AddPage(string name, DropdownChild child)
        {
            if (Children.ContainsKey(name))
            {
                Logger.Error($"Duplicate page name for {Key}! {name}");
                return this;
            }

            if (child is DropdownChild.ConditionChild conditionChild)
            {
                if (conditionChild.Child.Key is ControlKey)
                    throw new ArgumentException("You know what you did.");
                if (conditionChild.Child.Parent is not null)
                    throw new InvalidOperationException("An object cannot be the child of two containers!");
                if (!conditionChild.Child.TrySetParent(this))
                    return this;

                Children[name] = child;

                foreach (Player viewer in GeneratedDropdowns.Keys)
                {
                    if (!conditionChild.Condition(viewer))
                        continue;

                    ASSNetworking.SendToPlayer(viewer);
                    Selected.Remove(viewer);
                }
            }
            else if (child is DropdownChild.StandardChild standardChild)
            {
                if (standardChild.Child.Key is ControlKey)
                    throw new ArgumentException("You know what you did.");
                if (standardChild.Child.Parent is not null)
                    throw new InvalidOperationException("An object cannot be the child of two containers!");
                if (!standardChild.Child.TrySetParent(this))
                {
                    Logger.Warn($"Failed to set parent of {standardChild.Child}");
                    return this;
                }

                Children[name] = child;

                ASSNetworking.SendToAll();
                Selected.Clear();
            }
            else
            {
                Children[name] = child;

                ASSNetworking.SendToAll();
                Selected.Clear();
            }

            return this;
        }

        public DropdownPage RemovePage(string name)
        {
            if (!Children.Remove(name, out DropdownChild child))
                return this;

            if (child is DropdownChild.ConditionChild conditionChild)
            {
                foreach (Player viewer in GeneratedDropdowns.Keys)
                {
                    if (!conditionChild.Condition(viewer))
                        continue;

                    ASSNetworking.SendToPlayer(viewer);
                    Selected.Remove(viewer);
                }
            }
            else
            {
                ASSNetworking.SendToAll();
                Selected.Clear();
            }

            return this;
        }

        public string GetSelected(Player viewer) => Selected.TryGetValue(viewer, out string? value) ? value : string.Empty;

        public override IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            if (Children.Count is 0)
            {
                Logger.Debug($"No children found under {Key}", Main.Debug);
                yield break;
            }

            Logger.Debug("Yielding dropdown", Main.Debug);
            yield return GeneratedDropdowns[viewer] = Generator(viewer);

            IASSObject? obj;
            if (!Selected.TryGetValue(viewer, out string? selected) || !Children.TryGetValue(selected, out DropdownChild child))
            {
                if (Children.TryGetValue(Default, out DropdownChild defaultChild))
                {
                    Logger.Debug("Yielding default child.", Main.Debug);
                    obj = defaultChild.GetChild(viewer);

                    if (obj is null)
                    {
                        Logger.Error("Invalid page selected!");
                        yield break;
                    }

                    obj.TrySetParent(this, false);
                    yield return obj;
                    yield break;
                }

                Logger.Debug("Breaking, no selection found.", Main.Debug);
                yield break;
            }

            Logger.Debug("Yielding child", Main.Debug);
            obj = child.GetChild(viewer);

            if (obj is null)
            {
                Logger.Error("Invalid page selected!");
                yield break;
            }

            obj.TrySetParent(this, false);
            yield return obj;
        }

        public void Dispose() => PlayerEvents.Left -= OnLeft;

        private void SetForeachDropdown(Action<DropdownInstance> action)
        {
            foreach (DropdownTemplate dropdown in GeneratedDropdowns.Values)
            {
                foreach (DropdownInstance instance in dropdown.Instances.Values)
                {
                    action(instance);
                }
            }
        }

        private DropdownTemplate Generator(Player viewer)
        {
            Logger.Debug($"Building Dropdown for {viewer.Nickname}...", Main.Debug);
            return DropdownTemplate
                .Create("Dropdown Page Dropdown")
                .WithHandler(Handler)
                .WithOptions(
                    Children.Where(kvp =>
                        {
                            Logger.Debug($"Can use {kvp.Value}?", Main.Debug);
                            if (kvp.Value is DropdownChild.ConditionChild conditionChild)
                            {
                                bool result = conditionChild.Condition(viewer);
                                Logger.Debug(result, Main.Debug);
                                return result;
                            }

                            Logger.Debug(true, Main.Debug);
                            return true;
                        })
                        .Select(kvp => kvp.Key)
                        .ToArray())
                .WithLabel(Label)
                .WithCollectionId(CollectionId)
                .WithHint(Hint)
                .WithEntryType(EntryType);
        }

        private void Handler(Player player, DropdownInstance dropdown)
        {
            Logger.Debug($"{player.Nickname} selected page {dropdown.OptionSelected}", Main.Debug);

            bool change;
            if (Selected.TryGetValue(player, out string value))
                change = value != dropdown.OptionSelected;
            else
                change = true;

            Selected[player] = dropdown.OptionSelected;

            if (dropdown.OptionSelected == Default)
            {
                if (Opened.Remove(player))
                {
                    if (SelectedPage.TryGetValue(player, out Stack<ASSPage> stack) && stack.TryPeek(out ASSPage? page) && page == this)
                    {
                        stack.Pop();

                        ASSNetworking.SendToPlayer(player);
                    }
                }
            }
            else
            {
                if (Opened.Add(player))
                {
                    if (SelectedPage.TryGetValue(player, out Stack<ASSPage> stack))
                    {
                        stack.Push(this);

                        ASSNetworking.SendToPlayer(player);
                    }
                }
                else if (change)
                    ASSNetworking.SendToPlayer(player);
            }
        }

        private void OnLeft(PlayerLeftEventArgs ev)
        {
            GeneratedDropdowns.Remove(ev.Player);
            Selected.Remove(ev.Player);
        }

        public abstract class DropdownChild
        {
            public static DropdownChild Create(Func<Player, IASSObject> generator) => new GeneratorChild(generator);

            public static DropdownChild Create(IASSObject child, Predicate<Player> condition) => new ConditionChild(child, condition);

            public static DropdownChild Create(IASSObject child) => new StandardChild(child);

            internal abstract IASSObject? GetChild(Player viewer);

            public class ConditionChild : DropdownChild
            {
                internal ConditionChild(IASSObject child, Predicate<Player> condition)
                {
                    Child = child;
                    Condition = condition;
                }

                public IASSObject Child { get; }

                public Predicate<Player> Condition { get; }

                internal override IASSObject? GetChild(Player viewer) => Condition(viewer) ? Child : null;
            }

            public class GeneratorChild : DropdownChild
            {
                internal GeneratorChild(Func<Player, IASSObject> generator) => Generator = generator;

                public Func<Player, IASSObject> Generator { get; }

                internal override IASSObject GetChild(Player viewer) => Generator(viewer);
            }

            public class StandardChild : DropdownChild
            {
                internal StandardChild(IASSObject child) => Child = child;

                public IASSObject Child { get; }

                internal override IASSObject GetChild(Player viewer) => Child;
            }
        }
    }
}