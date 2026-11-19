namespace ASS.Features.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ASS.Features.Attributes;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Handlers;
    using LabApi.Features.Wrappers;

    public class PlayerParser : ASSParser, IHeaderParser, IDisposable
    {
        private Player[] players;

        public PlayerParser(string key)
            : base(key)
        {
            players = Player.ReadyList.ToArray();
            Dropdown.Options = players.Select(p => $"{p.Nickname} ({p.PlayerId})").ToArray();

            PlayerEvents.Joined += OnPlayerJoined;
            PlayerEvents.Left += OnPlayerLeft;

            Dropdown.WithHandler((player, dropdown) =>
            {
                if (IgnoreInitialResponses && dropdown.IsInitialResponse)
                    return;

                OnModified?.Invoke(player);
            });
        }

        public HeaderTemplate Header { get; } = HeaderTemplate.Create("Header");

        public DropdownTemplate Dropdown { get; } = DropdownTemplate
            .Create("Dropdown")
            .WithLabel("Players");

        public override IEnumerable<IASSObject> GetChildren(Player viewer) => [Header, Dropdown];

        public override void Transform(IEnumerable<IParserTransform> transforms)
        {
            foreach (IParserTransform transform in transforms)
            {
                if (transform is HeaderTransformAttribute header)
                    header.Apply(Header);
            }
        }

        public override bool TryParse(Player viewer, out object? value)
        {
            value = null;
            if (!Dropdown.Instances.TryGetValue(viewer, out DropdownInstance? instance))
                return false;

            value = players[instance.IndexSelected];
            return true;
        }

        public void Dispose()
        {
            PlayerEvents.Joined -= OnPlayerJoined;
            PlayerEvents.Left -= OnPlayerLeft;
        }

        private void OnPlayerJoined(PlayerJoinedEventArgs ev) => UpdateDropdown();

        private void OnPlayerLeft(PlayerLeftEventArgs ev) => UpdateDropdown();

        private void UpdateDropdown()
        {
            players = Player.ReadyList.ToArray();
            string[] options = Dropdown.Options = players.Select(p => $"{p.Nickname} ({p.PlayerId})").ToArray();

            foreach (DropdownInstance instance in Dropdown.Instances.Values)
                instance.Options = options;
        }
    }
}