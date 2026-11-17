namespace ASS.Collections.Features
{
    using System;
    using System.Collections.Generic;

    using ASS.Features;
    using ASS.Features.Attributes;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    [Fluent]
    public partial class Confirmation
    {
        private readonly HeaderTemplate header = HeaderTemplate
            .Create("ASS.Collections Confirmation Header");

        private readonly ButtonTemplate confirmButton = ButtonTemplate
            .Create("ASS.Collections Confirmation Confirm Button");

        private readonly ButtonTemplate cancelButton = ButtonTemplate
            .Create("ASS.Collections Confirmation Cancel Button");

        private readonly IASSObject[] templates;

        private Confirmation()
        {
            Message = string.Empty;

            confirmButton.WithHandler((player, _) =>
            {
                Confirm?.Invoke(player);
                Confirmations.Remove(player);
            });
            cancelButton.WithHandler((player, _) =>
            {
                Cancel?.Invoke(player);
                Confirmations.Remove(player);
            });

            templates = [header, confirmButton, cancelButton];
        }

        [Fluent]
        public string Message
        {
            get;
            set
            {
                header.Label = value;
                field = value;
            }
        }

        [Fluent]
        public Action<Player>? Confirm { get; set; }

        [Fluent]
        public Action<Player>? Cancel { get; set; }

        internal static Dictionary<Player, Confirmation> Confirmations { get; } = new();

        public static Confirmation Create() => new();

        public Confirmation Show(Player viewer, bool force = false)
        {
            Confirmations[viewer] = this;

            if (force)
                ASSNetworking.SendToPlayerFull(viewer, forceLoad: true);
            else
                ASSNetworking.SetDirty(viewer);

            return this;
        }

        internal IEnumerable<IASSObject> Templates() => templates;
    }
}