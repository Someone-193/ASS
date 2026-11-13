namespace ASS.Collections.Features.Pages
{
    using System;
    using System.Collections.Generic;

    using ASS.Features;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    public class TwoButtonsPage : ASSPage
    {
        private readonly List<IASSObject> children = [];

        private TwoButtonsPage(string key)
            : base(key)
        {
            TwoButtons = TwoButtonsTemplate
                .Create(ControlKey)
                .WithHandler(Handler);
        }

        public TwoButtonsTemplate TwoButtons { get; }

        public Predicate<Player>? Condition { get; set; }

        public bool RightIsTarget { get; set; }

        public static TwoButtonsPage Create(string key) => new(key);

        public TwoButtonsPage AddChild(IASSObject child)
        {
            if (children.Contains(child) || !child.TrySetParent(this))
                return this;

            if (child.Key is ControlKey)
                throw new ArgumentException("You know what you did.");

            children.Add(child);
            return this;
        }

        public TwoButtonsPage RemoveChild(IASSObject child)
        {
            if (children.Remove(child))
                child.TrySetParent(null);

            return this;
        }

        public TwoButtonsPage ClearChildren()
        {
            foreach (IASSObject child in children)
                child.TrySetParent(null);

            children.Clear();
            return this;
        }

        public TwoButtonsPage WithChildren(params IASSObject[] newChildren) => WithChildren((IEnumerable<IASSObject>)newChildren);

        public TwoButtonsPage WithChildren(IEnumerable<IASSObject> newChildren)
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

        public TwoButtonsPage WithCondition(Predicate<Player> condition)
        {
            Condition = condition;
            return this;
        }

        public TwoButtonsPage WithLabel(string label)
        {
            TwoButtons.WithLabel(label);
            return this;
        }

        public TwoButtonsPage WithHint(string hint)
        {
            TwoButtons.WithHint(hint);
            return this;
        }

        public TwoButtonsPage WithCollectionId(byte collectionId)
        {
            TwoButtons.WithCollectionId(collectionId);
            return this;
        }

        public TwoButtonsPage WithLeftOpensPage()
        {
            RightIsTarget = false;
            return this;
        }

        public TwoButtonsPage WithRightOpensPage()
        {
            RightIsTarget = true;
            return this;
        }

        public TwoButtonsPage WithLeftOption(string leftOption)
        {
            TwoButtons.LeftOption = leftOption;
            return this;
        }

        public TwoButtonsPage WithRightOption(string rightOption)
        {
            TwoButtons.RightOption = rightOption;
            return this;
        }

        public override IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            if (Condition is not null && !Condition(viewer))
                yield break;

            if (Opened.Contains(viewer))
            {
                yield return TwoButtons;

                foreach (IASSObject child in children)
                    yield return child;
            }
            else
            {
                yield return TwoButtons;
            }
        }

        private void Handler(Player player, TwoButtonsInstance twoButtons)
        {
            if ((twoButtons.RightSelected && RightIsTarget) || (twoButtons.LeftSelected && !RightIsTarget))
            {
                if (!Opened.Add(player))
                    return;
                if (!SelectedPage.TryGetValue(player, out Stack<ASSPage> stack))
                    return;

                stack.Push(this);
            }
            else
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
            }

            ASSNetworking.SendToPlayer(player);
        }
    }
}