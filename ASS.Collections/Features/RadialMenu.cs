namespace ASS.Collections.Features
{
    using System;
    using System.Collections.Generic;
    using ASS.Features.Attributes;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Handlers;
    using LabApi.Features.Wrappers;
    using MEC;
    using UnityEngine;

    // TODO: hints r bad. Wait for displaykit
    [Fluent]
    public partial class RadialMenu : IASSContainer, IDisposable
    {
        private readonly CoroutineHandle coroutine;

        private readonly Dictionary<Player, Vector2> activeMenus = [];

        private RadialMenu(string key)
        {
            Key = key;

            coroutine = Timing.RunCoroutine(Coroutine());
            PlayerEvents.TogglingNoclip += OnTogglingNoclip;
        }

        /// <summary>
        /// Gets or sets a shared action controlling how hints are sent to users.
        /// </summary>
        public static Action<Player, string> DisplayHint { get; set; } = (player, hint) => player.SendHint(hint, 1);

        /// <summary>
        /// Gets or sets a shared action controlling how hints are cleared for users.
        /// </summary>
        public static Action<Player> RemoveHint { get; set; } = player => player.SendHint(string.Empty);

        public string Key { get; }

        [Fluent]
        public string? Label
        {
            get => Header.Label;
            set => Header.Label = value;
        }

        // make this call Compile and set some private field.
        [Fluent]
        public string[] Options
        {
            get
            {
                return field;
            }

            set
            {
                field = value;
            }
        } = [];

        [Fluent]
        public Action<Player, int, string>? Handler { get; set; }

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy => true;

        private HeaderTemplate Header { get; }
            = HeaderTemplate
                .Create("RadialMenu Header")
                .WithHint("Use the ALT key in game to toggle this menu!");

        public static RadialMenu Create(string key) => new(key);

        public IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            yield return Header;
        }

        public void Dispose()
        {
            Timing.KillCoroutines(coroutine);
            PlayerEvents.TogglingNoclip -= OnTogglingNoclip;
        }

        private IEnumerator<float> Coroutine()
        {
            while (true)
            {
                foreach (Player player in activeMenus.Keys)
                {
                    DisplayHint(player, string.Empty);
                }

                // display dot element or something
                yield return Timing.WaitForSeconds(1);
            }
        }

        private void OnTogglingNoclip(PlayerTogglingNoclipEventArgs ev)
        {
            // I.E. if this menu is not visible to the player.
            if (!Header.Instances.ContainsKey(ev.Player))
                return;

            if (activeMenus.TryAdd(ev.Player, Vector2.zero))
            {
                ev.IsAllowed = false;

                DisplayHint(ev.Player, string.Empty);
            }
            else if (activeMenus.Remove(ev.Player, out Vector2 value))
            {
                ev.IsAllowed = false;

                RemoveHint(ev.Player);

                // convert value -> selected option.
                Handler?.Invoke(ev.Player, 0, string.Empty);
            }
        }

        private string Compile(string[] options) => string.Join(", ", options);
    }
}