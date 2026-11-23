namespace ASS.Collections.Features.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class SetEditorHelper
    {
        private static readonly Dictionary<Type, MethodInfo> Resizers = new();
        private static readonly Dictionary<Type, MethodInfo> Empties = new();

        private static readonly Dictionary<Type, MethodInfo> Adders = new();
        private static readonly Dictionary<Type, MethodInfo> Removers = new();
        private static readonly Dictionary<Type, MethodInfo> Clearers = new();

        private static readonly Dictionary<Type, MethodInfo> RemoveAts = new();
        private static readonly Dictionary<Type, MethodInfo> SetItems = new();

        private static readonly Dictionary<Kvp, MethodInfo> ContainsKeys = new();
        private static readonly Dictionary<Kvp, MethodInfo> DictAdders = new();
        private static readonly Dictionary<Kvp, MethodInfo> DictClearers = new();
        private static readonly Dictionary<Kvp, MethodInfo> DictSetItems = new();
        private static readonly Dictionary<Kvp, MethodInfo> DictRemovers = new();

        private static readonly Dictionary<Kvp, MethodInfo> KvpKeyGetters = new();

        private static readonly MethodInfo ResizeMethod = typeof(Array).GetMethod(nameof(Array.Resize))!;
        private static readonly MethodInfo EmptyMethod = typeof(Array).GetMethod(nameof(Array.Empty))!;

        public static MethodInfo GetResizer(Type type)
        {
            if (Resizers.TryGetValue(type, out MethodInfo? method))
                return method;

            return Resizers[type] = ResizeMethod.MakeGenericMethod(type);
        }

        public static MethodInfo GetEmpty(Type type)
        {
            if (Empties.TryGetValue(type, out MethodInfo? method))
                return method;

            return Empties[type] = EmptyMethod.MakeGenericMethod(type);
        }

        public static MethodInfo GetICollectionAdder(Type type)
        {
            if (Adders.TryGetValue(type, out MethodInfo? method))
                return method;

            return Adders[type] = typeof(ICollection<>).MakeGenericType(type).GetMethod(nameof(ICollection<>.Add))!;
        }

        public static MethodInfo GetICollectionRemover(Type type)
        {
            if (Removers.TryGetValue(type, out MethodInfo? method))
                return method;

            return Removers[type] = typeof(ICollection<>).MakeGenericType(type).GetMethod(nameof(ICollection<>.Remove))!;
        }

        public static MethodInfo GetICollectionClear(Type type)
        {
            if (Clearers.TryGetValue(type, out MethodInfo? method))
                return method;

            return Clearers[type] = typeof(ICollection<>).MakeGenericType(type).GetMethod(nameof(ICollection<>.Clear))!;
        }

        public static MethodInfo GetListRemoveAt(Type type)
        {
            if (RemoveAts.TryGetValue(type, out MethodInfo? method))
                return method;

            return RemoveAts[type] = typeof(List<>).MakeGenericType(type).GetMethod(nameof(List<>.RemoveAt))!;
        }

        public static MethodInfo GetListSetItem(Type type)
        {
            if (SetItems.TryGetValue(type, out MethodInfo? method))
                return method;

            return SetItems[type] = typeof(List<>).MakeGenericType(type).GetMethod("set_Item")!;
        }

        public static MethodInfo GetDictionaryContainsKey(Type key, Type value)
        {
            Kvp kvp = new(key, value);
            if (ContainsKeys.TryGetValue(kvp, out MethodInfo? method))
                return method;

            return ContainsKeys[kvp] = typeof(Dictionary<,>).MakeGenericType(key, value).GetMethod(nameof(Dictionary<,>.ContainsKey))!;
        }

        public static MethodInfo GetDictionaryAdd(Type key, Type value)
        {
            Kvp kvp = new(key, value);
            if (DictAdders.TryGetValue(kvp, out MethodInfo? method))
                return method;

            return DictAdders[kvp] = typeof(Dictionary<,>).MakeGenericType(key, value).GetMethod(nameof(Dictionary<,>.Add))!;
        }

        public static MethodInfo GetDictionaryClear(Type key, Type value)
        {
            Kvp kvp = new(key, value);
            if (DictClearers.TryGetValue(kvp, out MethodInfo? method))
                return method;

            return DictClearers[kvp] = typeof(Dictionary<,>).MakeGenericType(key, value).GetMethod(nameof(Dictionary<,>.Clear))!;
        }

        public static MethodInfo GetDictionarySetItem(Type key, Type value)
        {
            Kvp kvp = new(key, value);
            if (DictSetItems.TryGetValue(kvp, out MethodInfo? method))
                return method;

            return DictSetItems[kvp] = typeof(Dictionary<,>).MakeGenericType(key, value).GetMethod("set_Item")!;
        }

        public static MethodInfo GetDictionaryRemove(Type key, Type value)
        {
            Kvp kvp = new(key, value);
            if (DictRemovers.TryGetValue(kvp, out MethodInfo? method))
                return method;

            return DictRemovers[kvp] = typeof(Dictionary<,>).MakeGenericType(key, value).GetMethod(nameof(Dictionary<,>.Remove), 0, [key])!;
        }

        public static MethodInfo GetKeyFromKvp(Type key, Type value)
        {
            Kvp kvp = new(key, value);
            if (KvpKeyGetters.TryGetValue(kvp, out MethodInfo? method))
                return method;

            return KvpKeyGetters[kvp] = typeof(KeyValuePair<,>).MakeGenericType(key, value).GetProperty(nameof(KeyValuePair<,>.Key))!.GetMethod;
        }

        private readonly record struct Kvp(Type Key, Type Value);
    }
}