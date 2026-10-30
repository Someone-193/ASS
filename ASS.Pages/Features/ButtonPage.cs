namespace ASS.Pages.Features
{
    using System;
    using System.Collections.Generic;

    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    public class ButtonPage : ASSPage
    {
        private ButtonPage(string key, IASSObject child)
            : base(key)
        {
            Child = child;

            OpenButton = ButtonTemplate
                .Create(ControlKey)
                .WithHandler(Open);

            ExitButton = ButtonTemplate
                .Create(ControlKey)
                .WithHandler(Exit);
        }

        public IASSObject Child { get; }

        public ButtonTemplate OpenButton { get; }

        public ButtonTemplate ExitButton { get; }

        public static ButtonPage Create(string key, IASSObject child)
        {
            if (child.Key is ControlKey)
                throw new ArgumentException("You know what you did.");

            ButtonPage page = new(key, child);

            return page;
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

        public ButtonPage WithButtonText(string buttonText)
        {
            OpenButton.WithButtonText(buttonText);
            ExitButton.WithButtonText(buttonText);
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
            if (Opened.Contains(viewer))
            {
                yield return ExitButton;
                yield return Child;
            }
            else
            {
                yield return OpenButton;
            }
        }

        private void Open(Player player, ButtonInstance button)
        {
            if (Opened.Add(player))
            {
                if (SelectedPage.TryGetValue(player, out Stack<ASSPage> stack))
                    stack.Push(this);
            }
        }

        private void Exit(Player player, ButtonInstance button)
        {
            if (Opened.Remove(player))
            {
                if (SelectedPage.TryGetValue(player, out Stack<ASSPage> stack) && stack.TryPeek(out ASSPage? page) && page == this)
                    stack.Pop();
            }
        }
    }
}