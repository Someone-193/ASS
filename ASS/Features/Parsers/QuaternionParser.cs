namespace ASS.Features.Parsers
{
    using System.Collections.Generic;

    using ASS.Features.Attributes;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using UnityEngine;

    public sealed class QuaternionParser : ASSParser, IHeaderParser
    {
        public QuaternionParser(string key)
            : base(key)
        {
            SX.WithHandler(Handler());
            SY.WithHandler(Handler());
            SZ.WithHandler(Handler());

            return;

            InstanceHandler<SliderInstance> Handler()
                => (player, slider) =>
                {
                    if (IgnoreInitialResponses && slider.IsInitialResponse)
                        return;

                    OnModified?.Invoke(player);
                };
        }

        public HeaderTemplate Header { get; } = HeaderTemplate.Create("ASS Quaternion Parser Header");

        public SliderTemplate SX { get; } = SliderTemplate
            .Create("ASS Quaternion Parser SX")
            .WithMinValue(-180)
            .WithMaxValue(180)
            .WithLabel("x (-180 <-> 180)");

        public SliderTemplate SY { get; } = SliderTemplate
            .Create("ASS Quaternion Parser SY")
            .WithMinValue(-180)
            .WithMaxValue(180)
            .WithLabel("y (-180 <-> 180)");

        public SliderTemplate SZ { get; } = SliderTemplate
            .Create("ASS Quaternion Parser SZ")
            .WithMinValue(-180)
            .WithMaxValue(180)
            .WithLabel("z (-180 <-> 180)");

        public override IEnumerable<IASSObject> GetChildren(Player viewer) => [Header, SX, SY, SZ];

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

            if (!SX.Instances.TryGetValue(viewer, out SliderInstance i1) ||
                !SY.Instances.TryGetValue(viewer, out SliderInstance i2) ||
                !SZ.Instances.TryGetValue(viewer, out SliderInstance i3))
                return false;

            value = Quaternion.Euler(i1.Value, i2.Value, i3.Value);

            return true;
        }
    }
}