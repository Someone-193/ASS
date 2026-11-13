namespace ASS.Collections.Features.Pages
{
    using System;
    using System.Collections.Generic;

    using ASS.Features;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    public class ButtonPage : ASSPage
    {
        private readonly List<IASSObject> children = [];

        private ButtonPage(string key)
            : base(key)
        {
            OpenButton = ButtonTemplate
                .Create(ControlKey)
                .WithHandler(Open);

            ExitButton = ButtonTemplate
                .Create(ControlKey)
                .WithHandler(Exit);
        }

        public Predicate<Player>? Condition { get; set; }

        public ButtonTemplate OpenButton { get; }

        public ButtonTemplate ExitButton { get; }

        public static ButtonPage Create(string key) => new(key);

        public ButtonPage AddChild(IASSObject child)
        {
            if (children.Contains(child) || !child.TrySetParent(this))
                return this;

            if (child.Key is ControlKey)
                throw new ArgumentException("You know what you did.");

            children.Add(child);
            return this;
        }

        public ButtonPage RemoveChild(IASSObject child)
        {
            if (children.Remove(child))
                child.TrySetParent(null);

            return this;
        }

        public ButtonPage ClearChildren()
        {
            foreach (IASSObject child in children)
                child.TrySetParent(null);

            children.Clear();
            return this;
        }

        public ButtonPage WithChildren(params IASSObject[] newChildren) => WithChildren((IEnumerable<IASSObject>)newChildren);

        public ButtonPage WithChildren(IEnumerable<IASSObject> newChildren)
        {
            foreach (IASSObject child in newChildren)
            {
                if (child.Parent is not null)
                    throw new InvalidOperationException("An object cannot be the child of two containers!");

                if (!child.TrySetParent(this))
                    continue;

                if (child.Key is ControlKey)
                    throw new ArgumentException("You know what you did.");

                children.Add(child);
            }

            return this;
        }

        public ButtonPage WithCondition(Predicate<Player> condition)
        {
            Condition = condition;
            return this;
        }

        public ButtonPage WithLabel(string label)
        {
            OpenButton.WithLabel(label);
            ExitButton.WithLabel(label);
            return this;
        }

        public ButtonPage WithHint(string hint)
        {
            OpenButton.WithHint(hint);
            ExitButton.WithHint(hint);
            return this;
        }

        public ButtonPage WithCollectionId(byte collectionId)
        {
            OpenButton.WithCollectionId(collectionId);
            ExitButton.WithCollectionId(collectionId);
            return this;
        }

        public ButtonPage WithButtonText(string openButtonText, string exitButtonText)
        {
            OpenButton.WithButtonText(openButtonText);
            ExitButton.WithButtonText(exitButtonText);
            return this;
        }

        public ButtonPage WithHoldTime(float holdTime)
        {
            OpenButton.WithHoldTime(holdTime);
            ExitButton.WithHoldTime(holdTime);
            return this;
        }

        public override IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            if (Condition is not null && !Condition(viewer))
                yield break;

            if (Opened.Contains(viewer))
            {
                yield return ExitButton;

                foreach (IASSObject child in children)
                    yield return child;
            }
            else
            {
                yield return OpenButton;
            }
        }

        // these are public because the Button does not carry a state.
        public void Open(Player player)
        {
            if (!Opened.Add(player))
                return;
            if (!SelectedPage.TryGetValue(player, out Stack<ASSPage> stack))
                return;

            stack.Push(this);

            ASSNetworking.SendToPlayer(player);
        }

        public void Exit(Player player)
        {
            if (!Opened.Remove(player))
                return;
            if (!SelectedPage.TryGetValue(player, out Stack<ASSPage> stack))
                return;
            if (!stack.TryPeek(out ASSPage? page))
                return;
            if (page != this)
                return;

            stack.Pop();

            ASSNetworking.SendToPlayer(player);
        }

        private void Open(Player player, ButtonInstance button) => Open(player);

        private void Exit(Player player, ButtonInstance button) => Exit(player);
    }
}