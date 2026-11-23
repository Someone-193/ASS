namespace ASS.Features.Parsers
{
    extern alias oldSystem;

    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reflection;

    using LabApi.Features.Wrappers;

    using oldSystem::System.Net;

    /// <summary>
    /// Represents an instantiable parser, and other helper methods associated with parsing.
    /// </summary>
    public readonly record struct ParserTemplate
    {
        public const string NullKeyword = "<color=#6C95EB>null</color>";

        private static readonly MethodInfo ParseEnumMethod = typeof(ParserTemplate).GetMethod(nameof(ParseEnum), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException("Failed to find ParseEnum<T> method!");

        private readonly Func<string, ASSParser> generator;
        private readonly Func<object?, string>? print;

        public ParserTemplate(Func<string, ASSParser> generator, Func<object?, string>? print = null, string? kind = null)
        {
            this.generator = generator;
            this.print = print;
            Kind = kind;
        }

        public string? Kind { get; }

        public static ParserTemplate StandardTextTemplate<T>(TryParseDelegate<T> method) => new(str => new TextParser<T>(str, method));

        public static ParserTemplate CreateParserForEnum(Type enumType) => (ParserTemplate)ParseEnumMethod.MakeGenericMethod(enumType).Invoke(null, null);

        public ASSParser Instantiate(string key) => generator(key);

        public string Print(object? obj) => print?.Invoke(obj) ?? obj?.ToString() ?? NullKeyword;

        // IF ONLY WE HAD .NET 10
        internal static Dictionary<Clipboard.Key, ParserTemplate> DefaultParsers()
        {
            Dictionary<Clipboard.Key, ParserTemplate> value = new()
            {
                // text
                [new Clipboard.Key(typeof(char))] = StandardTextTemplate<char>(char.TryParse),
                [new Clipboard.Key(typeof(string))] = StandardTextTemplate<string>(StrHelper),

                // numbers
                [new Clipboard.Key(typeof(byte))] = StandardTextTemplate<byte>(byte.TryParse),
                [new Clipboard.Key(typeof(sbyte))] = StandardTextTemplate<byte>(byte.TryParse),
                [new Clipboard.Key(typeof(short))] = StandardTextTemplate<short>(short.TryParse),
                [new Clipboard.Key(typeof(ushort))] = StandardTextTemplate<ushort>(ushort.TryParse),
                [new Clipboard.Key(typeof(int))] = StandardTextTemplate<int>(int.TryParse),
                [new Clipboard.Key(typeof(uint))] = StandardTextTemplate<uint>(uint.TryParse),
                [new Clipboard.Key(typeof(long))] = StandardTextTemplate<long>(long.TryParse),
                [new Clipboard.Key(typeof(ulong))] = StandardTextTemplate<ulong>(ulong.TryParse),
                [new Clipboard.Key(typeof(float))] = StandardTextTemplate<float>(float.TryParse),
                [new Clipboard.Key(typeof(double))] = StandardTextTemplate<double>(double.TryParse),
                [new Clipboard.Key(typeof(decimal))] = StandardTextTemplate<decimal>(decimal.TryParse),
                [new Clipboard.Key(typeof(BigInteger))] = StandardTextTemplate<BigInteger>(BigInteger.TryParse),

                // times
                [new Clipboard.Key(typeof(DateTime))] = StandardTextTemplate<DateTime>(DateTime.TryParse),
                [new Clipboard.Key(typeof(DateTimeOffset))] = StandardTextTemplate<DateTimeOffset>(DateTimeOffset.TryParse),
                [new Clipboard.Key(typeof(TimeSpan))] = StandardTextTemplate<TimeSpan>(TimeSpan.TryParse),

                // identifiers
                [new Clipboard.Key(typeof(IPAddress))] = StandardTextTemplate<IPAddress>(IPAddress.TryParse),
                [new Clipboard.Key(typeof(Guid))] = StandardTextTemplate<Guid>(Guid.TryParse),
                [new Clipboard.Key(typeof(Version))] = StandardTextTemplate<Version>(Version.TryParse),

                // logic
                [new Clipboard.Key(typeof(bool))] = new ParserTemplate(str => new BoolParser(str)),

                // custom
                [new Clipboard.Key(typeof(Player))] = new ParserTemplate(str => new PlayerParser(str), obj =>
                {
                    if (obj is Player player)
                        return $"{player.Nickname} ({player.PlayerId})";

                    return obj?.ToString() ?? NullKeyword;
                }),

                [new Clipboard.Key(typeof(UnityEngine.Vector3))] = new ParserTemplate(str => new Vector3Parser(str)),

                [new Clipboard.Key(typeof(UnityEngine.Quaternion))] = new ParserTemplate(str => new QuaternionParser(str), obj =>
                {
                    if (obj is UnityEngine.Quaternion quaternion)
                        return quaternion.eulerAngles.ToString();

                    return obj?.ToString() ?? NullKeyword;
                }),

                [new Clipboard.Key(typeof(UnityEngine.Color))] = new ParserTemplate(str => new ColorParser(str)),
            };

            return value;
        }

        private static ParserTemplate ParseEnum<TEnum>()
            where TEnum : struct, Enum
            => new(str => new EnumParser<TEnum>(str));

        private static bool StrHelper(string input, out string output)
        {
            output = input;
            return true;
        }
    }
}