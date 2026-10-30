namespace ASS.Pages.Features
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
        private TwoButtonsPage(string key, IASSObject child)
            : base(key)
        {
            Child = child;

            TwoButtons = TwoButtonsTemplate
                .Create(ControlKey)
                .WithHandler(Handler);
        }

        public IASSObject Child { get; }

        public TwoButtonsTemplate TwoButtons { get; }

        public bool RightIsTarget { get; set; }

        public static TwoButtonsPage Create(string key, IASSObject child)
        {
            if (child.Key is ControlKey)
                throw new ArgumentException("You know what you did.");

            TwoButtonsPage page = new(key, child);

            return page;
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
            if (Opened.Contains(viewer))
            {
                yield return TwoButtons;
                yield return Child;
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
                if (Opened.Add(player))
                {
                    if (SelectedPage.TryGetValue(player, out Stack<ASSPage> stack))
                    {
                        stack.Push(this);

                        ASSNetworking.SendToPlayer(player);
                    }
                }
            }
            else
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
        }
    }
}