namespace ASS.Features.Parsers
{
    using System;
    using System.Collections.Generic;

    using ASS.Features.Attributes;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using UnityEngine;

    public class Vector3Parser : ASSParser, IHeaderParser
    {
        private readonly Dictionary<Player, Instance> instances = new();

        private readonly TwoButtonsTemplate twoButtons = TwoButtonsTemplate
            .Create("ASS Vector3 Parser TwoButtons")
            .WithLeftOption("Text Inputs")
            .WithRightOption("Sliders");

        private readonly TextInputTemplate iX = TextInputTemplate.Create("ASS Vector3 Parser IX");

        private readonly TextInputTemplate iy = TextInputTemplate.Create("ASS Vector3 Parser IY");

        private readonly TextInputTemplate iz = TextInputTemplate.Create("ASS Vector3 Parser IZ");

        public Vector3Parser(string key)
            : base(key)
        {
            twoButtons.WithHandler((player, setting) =>
            {
                if (IgnoreInitialResponses && setting.IsInitialResponse)
                    return;

                Instance instance = instances.GetOrAdd(player, Factory);

                if (!(setting.RightSelected ^ instance.UseSliders))
                    return;

                instance.UseSliders = setting.RightSelected;
                ASSNetworking.SetDirty(player);
            });

            iX.WithHandler(Handler(instance => instance.SX));
            iy.WithHandler(Handler(instance => instance.SY));
            iz.WithHandler(Handler(instance => instance.SZ));

            return;

            InstanceHandler<TextInputInstance> Handler(Func<Instance, SliderTemplate> getTemplate)
                => (player, input) =>
                {
                    if (IgnoreInitialResponses && input.IsInitialResponse)
                        return;

                    if (!float.TryParse(input.InputtedText, out float value))
                        return;

                    Instance instance = instances.GetOrAdd(player, Factory);
                    SliderTemplate template = getTemplate(instance);
                    template.MinValue = value - 5;
                    template.MaxValue = value + 5;

                    OnModified?.Invoke(player);
                };
        }

        public HeaderTemplate Header { get; } = HeaderTemplate.Create("ASS Vector3 Parser Header");

        public override IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            yield return Header;
            yield return twoButtons;

            Instance instance = instances.GetOrAdd(viewer, Factory);

            if (instance.UseSliders)
            {
                yield return instance.SX;
                yield return instance.SY;
                yield return instance.SZ;
            }
            else
            {
                yield return iX;
                yield return iy;
                yield return iz;
            }
        }

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

            Instance instance = instances.GetOrAdd(viewer, Factory);

            if (instance.UseSliders)
            {
                if (!instance.SX.Instances.TryGetValue(viewer, out SliderInstance i1) ||
                    !instance.SY.Instances.TryGetValue(viewer, out SliderInstance i2) ||
                    !instance.SZ.Instances.TryGetValue(viewer, out SliderInstance i3))
                    return false;

                value = new Vector3(i1.Value, i2.Value, i3.Value);
            }
            else
            {
                if (!iX.Instances.TryGetValue(viewer, out TextInputInstance i1) ||
                    !iy.Instances.TryGetValue(viewer, out TextInputInstance i2) ||
                    !iz.Instances.TryGetValue(viewer, out TextInputInstance i3))
                    return false;

                if (!float.TryParse(i1.InputtedText, out float f1) ||
                    !float.TryParse(i2.InputtedText, out float f2) ||
                    !float.TryParse(i3.InputtedText, out float f3))
                    return false;

                value = new Vector3(f1, f2, f3);
            }

            return true;
        }

        private Instance Factory()
        {
            Instance instance = new();

            InstanceHandler<SliderInstance> handler = (player, slider) =>
            {
                if (IgnoreInitialResponses && slider.IsInitialResponse)
                    return;

                OnModified?.Invoke(player);
            };

            instance.SX.WithHandler(handler);
            instance.SY.WithHandler(handler);
            instance.SZ.WithHandler(handler);

            return instance;
        }

        private class Instance
        {
            public bool UseSliders { get; set; }

            public SliderTemplate SX { get; } = SliderTemplate.Create("ASS Vector3 Parser SX");

            public SliderTemplate SY { get; } = SliderTemplate.Create("ASS Vector3 Parser SY");

            public SliderTemplate SZ { get; } = SliderTemplate.Create("ASS Vector3 Parser SZ");
        }
    }
}