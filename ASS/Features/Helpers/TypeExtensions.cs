namespace ASS.Features.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class TypeExtensions
    {
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

        public static string ProperTypeName(this Type type)
        {
            if (Aliases.TryGetValue(type, out string? alias))
                return alias;

            if (type.IsArray)
            {
                Type elementType = type.GetElementType()!;
                return elementType.ProperTypeName() + "[]";
            }

            if (type.IsConstructedGenericType)
            {
                string name = type.Name.Split('`')[0];
                string args = string.Join(", ", type.GenericTypeArguments.Select(ProperTypeName));
                return name + '<' + args + '>';
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
    }
}