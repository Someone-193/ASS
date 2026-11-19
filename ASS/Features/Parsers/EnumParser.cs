namespace ASS.Features.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using ASS.Features.Attributes;
    using ASS.Features.Helpers;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Unity.Collections.LowLevel.Unsafe;

    public class EnumParser<TEnum> : ASSParser, IHeaderParser
        where TEnum : struct, Enum
    {
        private readonly IASSObject[] children;
        private readonly Dictionary<string, TEnum> nameToValue = new();
        private readonly bool flags;

        public EnumParser(string key, bool flags)
            : base(key)
        {
            this.flags = flags;
            TEnum[] values = EnumUtils<TEnum>.Values;
            string[] names = EnumUtils<TEnum>.Names;

            if (values.Length != names.Length)
                throw new InvalidOperationException();

            for (int i = 0; i < values.Length; i++)
                nameToValue.Add(names[i], values[i]);

            if (flags)
                children = new IASSObject[values.Length + 1];
            else
                children = new IASSObject[2];

            children[0] = Header;

            InitializeChildren();
        }

        public EnumParser(string key)
            : this(key, typeof(TEnum).GetCustomAttribute(typeof(FlagsAttribute)) is not null)
        {
        }

        public HeaderTemplate Header { get; } = HeaderTemplate.Create("Header");

        public override IEnumerable<IASSObject> GetChildren(Player viewer) => children;

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
            if (flags)
            {
                TEnum enumValue = default;

                for (int i = 1; i < children.Length; i++)
                {
                    if (children[i] is not TwoButtonsTemplate twoButtons)
                        throw new InvalidOperationException();

                    if (!twoButtons.Instances.TryGetValue(viewer, out TwoButtonsInstance instance))
                        return false;

                    if (instance.LeftSelected && nameToValue.TryGetValue(twoButtons.Key, out TEnum newValue))
                        enumValue = Or(enumValue, newValue);
                }

                value = enumValue;
            }
            else
            {
                if (children[1] is not DropdownTemplate dropdown)
                    throw new InvalidOperationException();

                if (!dropdown.Instances.TryGetValue(viewer, out DropdownInstance instance))
                    return false;

                if (!nameToValue.TryGetValue(instance.OptionSelected, out TEnum newValue))
                    return false;

                value = newValue;
            }

            return true;
        }

        private static TEnum Or(TEnum left, TEnum right)
        {
            switch (UnsafeUtility.SizeOf<TEnum>())
            {
                case 1:
                {
                    byte result = (byte)(UnsafeUtility.As<TEnum, byte>(ref left) | UnsafeUtility.As<TEnum, byte>(ref right));
                    return UnsafeUtility.As<byte, TEnum>(ref result);
                }

                case 2:
                {
                    ushort result = (ushort)(UnsafeUtility.As<TEnum, ushort>(ref left) | UnsafeUtility.As<TEnum, ushort>(ref right));
                    return UnsafeUtility.As<ushort, TEnum>(ref result);
                }

                case 4:
                {
                    uint result = UnsafeUtility.As<TEnum, uint>(ref left) | UnsafeUtility.As<TEnum, uint>(ref right);
                    return UnsafeUtility.As<uint, TEnum>(ref result);
                }

                case 8:
                {
                    ulong result = UnsafeUtility.As<TEnum, ulong>(ref left) | UnsafeUtility.As<TEnum, ulong>(ref right);
                    return UnsafeUtility.As<ulong, TEnum>(ref result);
                }

                default:
                    throw new NotSupportedException($"Enum size {UnsafeUtility.SizeOf<TEnum>()} is not supported.");
            }
        }

        private void InitializeChildren()
        {
            if (flags)
            {
                int i = 1;
                foreach (KeyValuePair<string, TEnum> kvp in nameToValue)
                {
                    children[i++] = TwoButtonsTemplate
                        .Create(kvp.Key)
                        .WithLabel(kvp.Key)
                        .WithLeftOption("On")
                        .WithRightOption("Off")
                        .WithDefaultRightSelected()
                        .WithHandler((player, twoButtons) =>
                        {
                            if (IgnoreInitialResponses && twoButtons.IsInitialResponse)
                                return;

                            OnModified?.Invoke(player);
                        });
                }
            }
            else
            {
                children[1] = DropdownTemplate
                    .Create("EnumParser dropdown")
                    .WithLabel($"{typeof(TEnum).ProperTypeName()} Value")
                    .WithOptions(nameToValue.Keys.ToArray())
                    .WithHandler((player, dropdown) =>
                    {
                        if (IgnoreInitialResponses && dropdown.IsInitialResponse)
                            return;

                        OnModified?.Invoke(player);
                    });
            }
        }
    }
}