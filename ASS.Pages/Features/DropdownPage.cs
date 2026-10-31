namespace ASS.Pages.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ASS.Features;
    using ASS.Features.Containers;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;
    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;
    using UserSettings.ServerSpecific;

    // this is a really freaking complicated class due to the functionality it needs.
    public class DropdownPage : ASSPage
    {
        private DropdownPage(string key, string defaultPage, IASSObject defaultChild)
            : base(key)
        {
            Default = defaultPage;

            Children[defaultPage] = DropdownChild.Create(defaultChild);
            GeneratedDropdown = PlayerMenu.Create(key, Generator);
        }

        public PlayerMenu GeneratedDropdown { get; }

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
            SetForeachDropdown(i => i.Label = label);
            return this;
        }

        public DropdownPage WithHint(string hint)
        {
            SetForeachDropdown(i => i.Hint = hint);
            return this;
        }

        public DropdownPage WithCollectionId(byte collectionId)
        {
            SetForeachDropdown(i => i.CollectionId = collectionId);
            return this;
        }

        public DropdownPage WithEntryType(SSDropdownSetting.DropdownEntryType entryType)
        {
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

                foreach (Player viewer in GeneratedDropdown.Children.Keys)
                {
                    if (!conditionChild.Condition(viewer))
                        continue;

                    GeneratedDropdown.Update(viewer);
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
                    return this;

                GeneratedDropdown.UpdateAll();
                Selected.Clear();
            }
            else
            {
                GeneratedDropdown.UpdateAll();
                Selected.Clear();
            }

            Children[name] = child;

            return this;
        }

        public DropdownPage RemovePage(string name)
        {
            if (!Children.Remove(name, out DropdownChild child))
                return this;

            if (child is DropdownChild.ConditionChild conditionChild)
            {
                foreach (Player viewer in GeneratedDropdown.Children.Keys)
                {
                    if (!conditionChild.Condition(viewer))
                        continue;

                    GeneratedDropdown.Update(viewer);
                    Selected.Remove(viewer);
                }
            }
            else
            {
                GeneratedDropdown.UpdateAll();
                Selected.Clear();
            }

            Selected.Clear();
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
            yield return GeneratedDropdown;

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

        private void SetForeachDropdown(Action<DropdownInstance> action)
        {
            foreach (List<IASSObject> child in GeneratedDropdown.Children.Values)
            {
                foreach (DropdownInstance instance in ((DropdownTemplate)child[0]).Instances.Values)
                {
                    action(instance);
                }
            }
        }

        private IEnumerable<IASSObject> Generator(Player viewer)
        {
            yield return DropdownTemplate
                .Create("Dropdown Page Dropdown")
                .WithHandler(Handler)
                .WithOptions(
                    Children.Where(kvp =>
                        {
                            if (kvp.Value is DropdownChild.ConditionChild conditionChild)
                                return conditionChild.Condition(viewer);
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
            }
        }

        public abstract class DropdownChild
        {
            public static DropdownChild Create(Func<Player, IASSObject> generator) => new GeneratorChild(generator);

            public static DropdownChild Create(IASSObject child, Predicate<Player> condition) => new ConditionChild(child, condition);

            public static DropdownChild Create(IASSObject child) => new StandardChild(child);

            internal abstract IASSObject? GetChild(Player viewer);

            internal class ConditionChild : DropdownChild
            {
                internal ConditionChild(IASSObject child, Predicate<Player> condition)
                {
                    Child = child;
                    Condition = condition;
                }

                internal IASSObject Child { get; }

                internal Predicate<Player> Condition { get; }

                internal override IASSObject? GetChild(Player viewer) => Condition(viewer) ? Child : null;
            }

            internal class GeneratorChild : DropdownChild
            {
                internal GeneratorChild(Func<Player, IASSObject> generator) => Generator = generator;

                internal Func<Player, IASSObject> Generator { get; }

                internal override IASSObject GetChild(Player viewer) => Generator(viewer);
            }

            internal class StandardChild : DropdownChild
            {
                internal StandardChild(IASSObject child) => Child = child;

                internal IASSObject Child { get; }

                internal override IASSObject GetChild(Player viewer) => Child;
            }
        }
    }
}