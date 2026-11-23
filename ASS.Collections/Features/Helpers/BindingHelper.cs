namespace ASS.Collections.Features.Helpers
{
    using System;
    using System.Reflection;

    using HarmonyLib;

    /// <summary>
    /// Helper class for <see cref="Builder"/> and <see cref="Binding"/>.
    /// </summary>
    public static class BindingHelper
    {
        private static readonly MethodInfo RefHelperMethod = typeof(BindingHelper).GetMethod(nameof(GenericRefHelper), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Failed to find ParseEnum<T> method!");
        private static readonly MethodInfo FieldRefAccessMethod = typeof(AccessTools).GetMethod(nameof(AccessTools.FieldRefAccess), 1, BindingFlags.Public | BindingFlags.Static, null, [typeof(Type), typeof(string)], null) ?? throw new InvalidOperationException();

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
    }
}