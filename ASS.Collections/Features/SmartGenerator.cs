namespace ASS.Collections.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ASS.Features.Containers;
    using ASS.Features.Interfaces;

    using LabApi.Features.Wrappers;

    /// <summary>
    /// Similar to <see cref="PlayerMenu"/>, but behaves as a <see cref="TemplateGenerator"/> with control over when the cached templates are considered "dirty".
    /// </summary>
    /// <remarks>
    /// <para>One way to use this, is to let this generator represent a menu for a particular instance of an object. (Say a Type named <b>Car</b>).</para>
    /// <para>When <see cref="Generator"/> is invoked then, you can call <see cref="SetState"/> to set the state of the player to the <b>Car</b> sent to the player.</para>
    /// <para>Now you can compare in <see cref="Validator"/> the last <b>Car</b> sent to the player versus a static <see cref="Dictionary{Player,Car}"/> you control.</para>
    /// <para>If <see cref="Validator"/> returns <see langword="null"/>, then new settings are generated and cached, and you update the player's State again.</para>
    /// </remarks>
    public class SmartGenerator : IASSContainer
    {
        private readonly Dictionary<Player, IASSObject[]> caches = new();
        private readonly Dictionary<Player, object?> states = new();

        private SmartGenerator(string key, ASSGenerator<SmartGenerator> generator, Func<Player, object?, bool> validator)
        {
            Key = key;
            Generator = generator;
            Validator = validator;
        }

        public string Key { get; }

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public ASSGenerator<SmartGenerator> Generator { get; set; }

        public Func<Player, object?, bool> Validator { get; set; }

        public static SmartGenerator Create(string key, ASSGenerator<SmartGenerator> generator, Func<Player, object?, bool> validator) => new(key, generator, validator);

        /// <summary>
        /// A method to set an internal state of this <see cref="SmartGenerator"/>. Lets you track useful variables to then check in your validator.
        /// </summary>
        /// <param name="player">The player whose state will be updated.</param>
        /// <param name="state">An arbitrary object provided in <see cref="Validator"/>.</param>
        public void SetState(Player player, object state) => states[player] = state;

        public void Invalidate(Player player) => states.Remove(player);

        public void InvalidateAll() => states.Clear();

        public IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            if (caches.TryGetValue(viewer, out IASSObject[] cache))
            {
                if (states.TryGetValue(viewer, out object? state))
                {
                    if (Validator(viewer, state))
                        return cache;
                }
            }

            return caches[viewer] = Generator(this, viewer).ToArray();
        }
    }
}