namespace ASS.Collections.Features.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class SetEditorHelper
    {
        private static readonly Dictionary<Type, MethodInfo> Resizers = new();
        private static readonly Dictionary<Type, MethodInfo> Adders = new();
        private static readonly Dictionary<Type, MethodInfo> Removers = new();
        private static readonly Dictionary<Type, MethodInfo> RemoveAts = new();
        private static readonly Dictionary<Type, MethodInfo> SetItems = new();

        private static readonly MethodInfo ResizeMethod = typeof(Array).GetMethod("Resize", BindingFlags.Public | BindingFlags.Static)!;

        public static MethodInfo GetResizer(Type type)
        {
            if (Resizers.TryGetValue(type, out MethodInfo? method))
                return method;

            return Resizers[type] = ResizeMethod.MakeGenericMethod(type);
        }

        public static MethodInfo GetICollectionAdder(Type type)
        {
            if (Adders.TryGetValue(type, out MethodInfo? method))
                return method;

            return Adders[type] = typeof(ICollection<>).MakeGenericType(type).GetMethod("Add", BindingFlags.Public | BindingFlags.Instance)!;
        }

        public static MethodInfo GetICollectionRemover(Type type)
        {
            if (Removers.TryGetValue(type, out MethodInfo? method))
                return method;

            return Removers[type] = typeof(ICollection<>).MakeGenericType(type).GetMethod("Remove", BindingFlags.Public | BindingFlags.Instance)!;
        }

        public static MethodInfo GetListRemoveAt(Type type)
        {
            if (RemoveAts.TryGetValue(type, out MethodInfo? method))
                return method;

            return RemoveAts[type] = typeof(List<>).MakeGenericType(type).GetMethod("RemoveAt", BindingFlags.Public | BindingFlags.Instance)!;
        }

        public static MethodInfo GetListSetItem(Type type)
        {
            if (SetItems.TryGetValue(type, out MethodInfo? method))
                return method;

            return SetItems[type] = typeof(List<>).MakeGenericType(type).GetMethod("set_Item", BindingFlags.Public | BindingFlags.Instance)!;
        }
    }
}