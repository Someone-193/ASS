namespace ASS.Pages.Features
{
    using System;
    using System.Collections.Generic;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;
    using LabApi.Features.Wrappers;

    public class Confirmation
    {
        private readonly HeaderTemplate header = HeaderTemplate
            .Create("ASS.Pages Confirmation Header");

        private readonly ButtonTemplate confirmButton = ButtonTemplate
            .Create("ASS.Pages Confirmation Confirm Button");

        private readonly ButtonTemplate cancelButton = ButtonTemplate
            .Create("ASS.Pages Confirmation Cancel Button");

        private readonly IASSObject[] templates;

        private Confirmation()
        {
            Message = string.Empty;

            confirmButton.WithHandler((player, _) =>
            {
                Confirm?.Invoke();
                Confirmations.Remove(player);
            });
            cancelButton.WithHandler((player, _) =>
            {
                Cancel?.Invoke();
                Confirmations.Remove(player);
            });

            templates = [header, confirmButton, cancelButton];
        }

        public string Message
        {
            get;
            set
            {
                header.Label = value;
                field = value;
            }
        }

        public Action? Confirm { get; set; }

        public Action? Cancel { get; set; }

        internal static Dictionary<Player, Confirmation> Confirmations { get; } = new();

        public static Confirmation Create() => new();

        public Confirmation WithMessage(string message)
        {
            Message = message;
            return this;
        }

        public Confirmation OnConfirm(Action confirm)
        {
            Confirm = confirm;
            return this;
        }

        public Confirmation OnCancel(Action cancel)
        {
            Cancel = cancel;
            return this;
        }

        public Confirmation Show(Player viewer)
        {
            Confirmations[viewer] = this;
            return this;
        }

        internal IEnumerable<IASSObject> Templates() => templates;
    }
}