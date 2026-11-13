namespace ASS.Collections.Features.Helpers
{
    extern alias oldSystem;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    using HarmonyLib;

    /// <summary>
    /// Helper class for <see cref="Builder"/> and <see cref="Binding"/>.
    /// </summary>
    public static class BindingHelper
    {
        private static readonly MethodInfo ParseEnumMethod = typeof(BindingHelper).GetMethod(nameof(ParseEnum), BindingFlags.Static | BindingFlags.Public) ?? throw new InvalidOperationException("Failed to find ParseEnum<T> method!");
        private static readonly MethodInfo RefHelperMethod = typeof(BindingHelper).GetMethod(nameof(GenericRefHelper), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Failed to find ParseEnum<T> method!");
        private static readonly MethodInfo FieldRefAccessMethod = typeof(AccessTools).GetMethod(nameof(AccessTools.FieldRefAccess), 1, BindingFlags.Public | BindingFlags.Static, null, [typeof(Type), typeof(string)], null) ?? throw new InvalidOperationException();

#pragma warning disable SA1025
        private static readonly Dictionary<Type, string> Aliases = new()
        {
            { typeof(bool),    "bool"    },
            { typeof(byte),    "byte"    },
            { typeof(sbyte),   "sbyte"   },
            { typeof(char),    "char"    },
            { typeof(decimal), "decimal" },
            { typeof(double),  "double"  },
            { typeof(float),   "float"   },
            { typeof(int),     "int"     },
            { typeof(uint),    "uint"    },
            { typeof(long),    "long"    },
            { typeof(ulong),   "ulong"   },
            { typeof(short),   "short"   },
            { typeof(ushort),  "ushort"  },
            { typeof(object),  "object"  },
            { typeof(string),  "string"  },
            { typeof(void),    "void"    },
        };
#pragma warning restore SA1025

        public delegate bool TryParseDelegate<T>(string str, out T result);

        public static Dictionary<Type, Func<string, ParserResponse>> GlobalParsers { get; } = DefaultParsers();

        public static string ProperTypeName(Type type)
        {
            if (Aliases.TryGetValue(type, out string? alias))
                return alias;

            if (type.IsArray)
            {
                Type elementType = type.GetElementType()!;
                return $"{ProperTypeName(elementType)}[]";
            }

            if (type.IsConstructedGenericType)
            {
                string name = type.Name.Split('`')[0];
                string args = string.Join(", ", type.GenericTypeArguments.Select(ProperTypeName));
                return $"{name}<{args}>";
            }

            return type.Name;
        }

        public static bool IsSet(this Type type)
        {
            if (type.IsArray)
                return true;

            if (!type.IsConstructedGenericType)
                return false;

            Type definition = type.GetGenericTypeDefinition();

            return definition == typeof(List<>) || definition == typeof(HashSet<>);
        }

        // IF ONLY WE HAD .NET 10
        public static Dictionary<Type, Func<string, ParserResponse>> DefaultParsers() => new()
            {
                // text
                [typeof(char)] = str => str.Length is not 1 ? ParserResponse.Error : new ParserResponse(str[0]),
                [typeof(string)] = str => new ParserResponse(str),

                // numbers
                [typeof(byte)] = str => StandardTryParseResponse<byte>(str, byte.TryParse),
                [typeof(sbyte)] = str => StandardTryParseResponse<byte>(str, byte.TryParse),
                [typeof(short)] = str => StandardTryParseResponse<short>(str, short.TryParse),
                [typeof(ushort)] = str => StandardTryParseResponse<ushort>(str, ushort.TryParse),
                [typeof(int)] = str => StandardTryParseResponse<int>(str, int.TryParse),
                [typeof(uint)] = str => StandardTryParseResponse<uint>(str, uint.TryParse),
                [typeof(long)] = str => StandardTryParseResponse<long>(str, long.TryParse),
                [typeof(ulong)] = str => StandardTryParseResponse<ulong>(str, ulong.TryParse),
                [typeof(float)] = str => StandardTryParseResponse<float>(str, float.TryParse),
                [typeof(double)] = str => StandardTryParseResponse<double>(str, double.TryParse),
                [typeof(decimal)] = str => StandardTryParseResponse<decimal>(str, decimal.TryParse),
                [typeof(BigInteger)] = str => StandardTryParseResponse<BigInteger>(str, BigInteger.TryParse),

                // times
                [typeof(DateTime)] = str => StandardTryParseResponse<DateTime>(str, DateTime.TryParse),
                [typeof(DateTimeOffset)] = str => StandardTryParseResponse<DateTimeOffset>(str, DateTimeOffset.TryParse),
                [typeof(TimeSpan)] = str => StandardTryParseResponse<TimeSpan>(str, TimeSpan.TryParse),

                // identifiers
                [typeof(oldSystem::System.Net.IPAddress)] = str => StandardTryParseResponse<oldSystem::System.Net.IPAddress>(str, oldSystem::System.Net.IPAddress.TryParse),
                [typeof(Guid)] = str => StandardTryParseResponse<Guid>(str, Guid.TryParse),
                [typeof(Version)] = str => StandardTryParseResponse<Version>(str, Version.TryParse),

                // logic
                [typeof(bool)] = str => StandardTryParseResponse<bool>(str, bool.TryParse),
            };

        public static ParserResponse StandardTryParseResponse<T>(string str, TryParseDelegate<T> method)
        {
            bool error = !method(str, out T result);
            return new ParserResponse(result, error);
        }

        public static ParserResponse ParseEnum<TEnum>(string str)
            where TEnum : struct, Enum
        {
            bool error = !Enum.TryParse(str, out TEnum result);
            return new ParserResponse(result, error);
        }

        public static Func<string, ParserResponse> CreateParserForEnum(Type enumType)
            => (Func<string, ParserResponse>)Delegate.CreateDelegate(typeof(Func<string, ParserResponse>), ParseEnumMethod.MakeGenericMethod(enumType));

        public static Action<object, object?> CreateRefSetter(FieldInfo field)
        {
            MethodInfo refMethod = FieldRefAccessMethod.MakeGenericMethod(field.FieldType);
            MethodInfo helperMethod = RefHelperMethod.MakeGenericMethod(field.FieldType);

            object fieldRef = refMethod.Invoke(null, [field.FieldType, field.Name]);
            return (Action<object, object?>)helperMethod.Invoke(null, [fieldRef]);
        }

        private static Action<object, object?> GenericRefHelper<TF>(object fieldRefObj)
        {
            AccessTools.FieldRef<object, TF> fieldRef = (AccessTools.FieldRef<object, TF>)fieldRefObj;
            return (instance, value) => fieldRef(instance) = (TF)value!;
        }

        public readonly record struct ParserResponse(object? Value, bool IsError = false)
        {
            public static ParserResponse Error { get; } = new(null, true);
        }
    }
}