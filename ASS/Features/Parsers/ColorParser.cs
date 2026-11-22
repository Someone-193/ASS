namespace ASS.Features.Parsers
{
    using System.Collections.Generic;
    using ASS.Features.Attributes;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;
    using LabApi.Features.Wrappers;
    using UnityEngine;

    public class ColorParser : ASSParser, IHeaderParser
    {
        private readonly TwoButtonsTemplate twoButtons = TwoButtonsTemplate
            .Create("ASS Color Parser TwoButtons")
            .WithLeftOption("Text Inputs")
            .WithRightOption("Sliders");

        private readonly TextInputTemplate ir = TextInputTemplate
            .Create("ASS Color Parser IR")
            .WithLabel("Red");

        private readonly TextInputTemplate ig = TextInputTemplate
            .Create("ASS Color Parser IG")
            .WithLabel("Green");

        private readonly TextInputTemplate ib = TextInputTemplate
            .Create("ASS Color Parser IB")
            .WithLabel("Blue");

        private readonly TextInputTemplate ia = TextInputTemplate
            .Create("ASS Color Parser IA")
            .WithLabel("Alpha");

        private readonly SliderTemplate sr = SliderTemplate
            .Create("ASS Color Parser SR")
            .WithMinValue(0)
            .WithMaxValue(1)
            .WithLabel("Red");

        private readonly SliderTemplate sg = SliderTemplate
            .Create("ASS Color Parser SG")
            .WithMinValue(0)
            .WithMaxValue(1)
            .WithLabel("Green");

        private readonly SliderTemplate sb = SliderTemplate
            .Create("ASS Color Parser SB")
            .WithMinValue(0)
            .WithMaxValue(1)
            .WithLabel("Blue");

        private readonly SliderTemplate sa = SliderTemplate
            .Create("ASS Color Parser SA")
            .WithMinValue(0)
            .WithMaxValue(1)
            .WithLabel("Alpha");

        private bool useSliders;

        public ColorParser(string key)
            : base(key)
        {
            twoButtons.WithHandler((player, setting) =>
            {
                if (IgnoreInitialResponses && setting.IsInitialResponse)
                    return;

                if (!(setting.RightSelected ^ useSliders))
                    return;

                useSliders = setting.RightSelected;
                ASSNetworking.SetDirty(player);
            });

            InstanceHandler<SliderInstance> sliderHandler = (player, slider) =>
            {
                if (IgnoreInitialResponses && slider.IsInitialResponse)
                    return;

                OnModified?.Invoke(player);
            };

            sr.WithHandler(sliderHandler);
            sg.WithHandler(sliderHandler);
            sb.WithHandler(sliderHandler);
            sa.WithHandler(sliderHandler);

            InstanceHandler<TextInputInstance> textInputHandler =
                (player, input) =>
                {
                    if (IgnoreInitialResponses && input.IsInitialResponse)
                        return;

                    OnModified?.Invoke(player);
                };

            ir.WithHandler(textInputHandler);
            ig.WithHandler(textInputHandler);
            ib.WithHandler(textInputHandler);
            ia.WithHandler(textInputHandler);
        }

        public HeaderTemplate Header { get; } = HeaderTemplate.Create("ASS Color Parser Header");

        public override IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            yield return Header;
            yield return twoButtons;

            if (useSliders)
            {
                yield return sr;
                yield return sg;
                yield return sb;
                yield return sa;
            }
            else
            {
                yield return ir;
                yield return ig;
                yield return ib;
                yield return ia;
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

            if (useSliders)
            {
                if (!sr.Instances.TryGetValue(viewer, out SliderInstance r) ||
                    !sg.Instances.TryGetValue(viewer, out SliderInstance g) ||
                    !sb.Instances.TryGetValue(viewer, out SliderInstance b) ||
                    !sa.Instances.TryGetValue(viewer, out SliderInstance a))
                    return false;

                value = new Color(r.Value, g.Value, b.Value, a.Value);
            }
            else
            {
                if (!ir.Instances.TryGetValue(viewer, out TextInputInstance i1) ||
                    !ig.Instances.TryGetValue(viewer, out TextInputInstance i2) ||
                    !ib.Instances.TryGetValue(viewer, out TextInputInstance i3) ||
                    !ia.Instances.TryGetValue(viewer, out TextInputInstance i4))
                    return false;

                if (!float.TryParse(i1.InputtedText, out float r) ||
                    !float.TryParse(i2.InputtedText, out float g) ||
                    !float.TryParse(i3.InputtedText, out float b) ||
                    !float.TryParse(i4.InputtedText, out float a))
                    return false;

                value = new Color(r, g, b, a);
            }

            return true;
        }
    }
}