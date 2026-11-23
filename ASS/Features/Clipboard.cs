namespace ASS.Features
{
    extern alias oldSystem;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using ASS.Features.Parsers;
    using LabApi.Features.Wrappers;

    /// <summary>
    /// A static generalized location for saving objects acquired from in-game commands or utilities.
    /// </summary>
    /// <remarks>
    /// <para>A good use case of this, is a gun that save the shot position to clipboard, then an ASS setting / collection to use that value.</para>
    /// <para>Imagine a menu for a plugin like MER, it could have its toolgun save to clipboard, then you could input that value into anything that takes a position.</para>
    /// </remarks>
    public static class Clipboard
    {
        private static readonly Dictionary<Player, Context> Values = new();

        /// <summary>
        /// Gets a <see cref="Dictionary{Type, ParserTemplate}"/> containing every registered <see cref="ParserTemplate"/>.
        /// </summary>
        private static Dictionary<Key, ParserTemplate> Parsers { get; } = ParserTemplate.DefaultParsers();

        public static bool RegisterParser(Type type, ParserTemplate parser, string? kind = null) => Parsers.TryAdd(new Key(type, kind), parser);

        public static bool TryGetParser(Type type, out ParserTemplate parser, string? kind = null)
        {
            if (Parsers.TryGetValue(new Key(type, kind), out parser))
                return true;

            if (type.IsEnum)
            {
                parser = ParserTemplate.CreateParserForEnum(type);
                RegisterParser(type, parser, kind);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Saves an object to a player's clipboard.
        /// </summary>
        /// <param name="obj">The object to save.</param>
        /// <param name="player">The target player.</param>
        /// <param name="kind">An optional string denoting what obj represents.</param>
        /// <remarks>
        /// <para>Consider calling <see cref="ASSNetworking.SendToPlayer"/> or <see cref="ASSNetworking.SetDirty"/> after using this method.</para>
        /// <para>Example use case.</para>
        /// type can be used to differentiate position vs scale for Vector3's.
        /// </remarks>
        public static void SaveToClipboard(this object obj, Player player, string? kind = null) => Values[player] = new Context(kind, obj);

        /// <summary>
        /// Gets an object from a player's clipboard.
        /// </summary>
        /// <param name="player">The player who has the value.</param>
        /// <param name="value">The found value.</param>
        /// <param name="kind">An optional string denoting what the found value should represent.</param>
        /// <typeparam name="T">The target type of object.</typeparam>
        /// <returns>Whether the target value could be found.</returns>
        /// <remarks>type can be used to differentiate position vs scale for Vector3's.</remarks>
        public static bool TryGetValue<T>(Player player, [NotNullWhen(true)] out T? value, string? kind = null)
        {
            if (Values.TryGetValue(player, out Context ctx) && ctx.Value is T val)
            {
                if (kind is not null && ctx.Type is not null && ctx.Type != kind)
                {
                    value = default;
                    return false;
                }

                value = val;
                return true;
            }

            value = default;
            return false;
        }

        public readonly record struct Key(Type Type, string? Kind = null);

        private readonly record struct Context(string? Type, object Value);
    }
}