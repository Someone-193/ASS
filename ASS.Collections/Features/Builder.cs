namespace ASS.Collections.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using ASS.Features;
    using ASS.Features.Attributes;
    using ASS.Features.Helpers;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Parsers;
    using ASS.Features.Templates;

    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;

    /// <summary>
    /// <see cref="Binding"/>, but for constructors.
    /// </summary>
    [Fluent]
    public partial class Builder : IASSContainer
    {
        private readonly List<ParameterInfo> parameters = [];
        private readonly List<IASSObject> objects = [];

        private Builder(string key) => Key = key;

        public string Key { get; }

        [Fluent]
        public ConstructorInfo? Constructor
        {
            get;
            set
            {
                if (value is not null && value.DeclaringType is null)
                    throw new ArgumentException("Cannot use a Builder for a constructor without a declared type!");

                bool change = field != value;
                field = value;

                if (change)
                    RebuildParameters();
            }
        }

        [Fluent]
        public float HoldTime
        {
            get;
            set
            {
                field = value;
                if (objects.Count is 0)
                    return;

                if (objects[objects.Count - 1] is not ButtonTemplate button)
                    return;

                button.HoldTime = value;

                foreach (ButtonInstance instance in button.Instances.Values)
                    instance.HoldTime = value;
            }
        }

        [Fluent]
        public Action<Player, object>? OnBuilt { get; set; }

        [Fluent(false)]
        public bool SuppressLogs { get; set; } = true;

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public static Builder Create(string key) => new(key);

        public IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            // re-use old templates unless rebuilt in RebuildParameters.
            if (objects.Count > 0)
                return objects;

            if (Constructor is null)
                return [];

            objects.Add(HeaderTemplate
                .Create("ASS.Collections Builder Header")
                .WithLabel($"{Constructor.DeclaringType!.Name} builder"));

            foreach (ParameterInfo parameter in parameters)
            {
                Type targetType = parameter.ParameterType;
                if (!Clipboard.TryGetParser(targetType, out ParserTemplate parserTemplate))
                {
                    if (!SuppressLogs)
                        Logger.Warn($"Cannot build type {Constructor.DeclaringType?.FullName ?? "null"} as no parser for {targetType.FullName} was found");
                    return [];
                }

                ASSParser parser = parserTemplate.Instantiate(parameter.Name);
                parser.Transform(parameter.GetCustomAttributes().OfType<IParserTransform>());

                if (parser is IHeaderParser headerParser)
                {
                    string label = $"{parameter.Name} ({parameter.ParameterType.ProperTypeName()}):";

                    if (parameter.HasDefaultValue)
                        label += " [" + (parameter.DefaultValue?.ToString() ?? "null") + ']';

                    headerParser.Header
                        .WithLabel(label)
                        .WithReducedPadding();
                }

                objects.Add(parser);

                // objects.Add(TextInputTemplate
                //     .Create(parameter.Name)
                //     .WithLabel(parameter.Name)
                //     .WithDefaultText(text)
                //     .WithPlaceholder(text));
            }

            objects.Add(ButtonTemplate
                .Create("ASS.Collections Builder Build Button")
                .WithLabel("Create")
                .WithHoldTime(HoldTime)
                .WithHandler((player, button) =>
                {
                    // it should be noted here all the index math just needs to ignore the first entry in templates. This includes this entry though as this is the buttons HANDLER.
                    object?[] args = new object?[objects.Count - 2];

                    for (int i = 1; i < objects.Count - 1; i++)
                    {
                        if (objects[i] is not ASSParser parser)
                        {
                            Logger.Error($"Invalid Builder state, expected ASSParser but found {objects[i]}!");

                            foreach (IASSObject template in objects)
                            {
                                Logger.Warn(template);
                            }

                            return;
                        }

                        if (!parser.TryParse(player, out object? obj))
                            return;

                        args[i - 1] = obj;
                    }

                    object instance = Constructor.Invoke(args);

                    if (OnBuilt is null)
                        return;

                    try
                    {
                        OnBuilt(player, instance);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }));

            return objects;
        }

        private void RebuildParameters()
        {
            parameters.Clear();
            objects.Clear();

            if (Constructor is null)
                return;

            parameters.AddRange(Constructor.GetParameters());
        }
    }
}