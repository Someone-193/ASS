namespace ASS.Collections.Features
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using ASS.Features.Helpers;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;

    /// <summary>
    /// <see cref="Binding"/>, but for constructors.
    /// </summary>
    public class Builder : IASSContainer
    {
        private readonly List<ParameterInfo> parameters = [];
        private readonly List<ASSTemplate> templates = [];

        private Builder(string key) => Key = key;

        public Dictionary<Type, Func<string, BindingHelper.ParserResponse>> Parsers { get; } = new();

        public string Key { get; }

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

        public float HoldTime
        {
            get;
            set
            {
                field = value;
                if (templates.Count is 0)
                    return;

                if (templates[templates.Count - 1] is not ButtonTemplate button)
                    return;

                button.HoldTime = value;

                foreach (ButtonInstance instance in button.Instances.Values)
                    instance.HoldTime = value;
            }
        }

        public Action<Player, object>? OnBuilt { get; set; }

        public bool SuppressLogs { get; set; } = true;

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public static Builder Create(string key) => new(key);

        public Builder WithConstructor(ConstructorInfo? constructor)
        {
            Constructor = constructor;
            return this;
        }

        public Builder WithOnBuilt(Action<Player, object>? onBuilt)
        {
            OnBuilt = onBuilt;
            return this;
        }

        public Builder WithHoldTime(float holdTime)
        {
            HoldTime = holdTime;
            return this;
        }

        public Builder WithoutSuppressLogs(bool suppressLogs = false)
        {
            SuppressLogs = suppressLogs;
            return this;
        }

        public IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            // re-use old templates unless rebuilt in RebuildParameters.
            if (templates.Count > 0)
                return templates;

            if (Constructor is null)
                return [];

            templates.Add(HeaderTemplate
                .Create("ASS.Collections Builder Header")
                .WithLabel($"{Constructor.DeclaringType!.Name} builder"));

            foreach (ParameterInfo parameter in parameters)
            {
                string text = parameter.DefaultValue?.ToString() ?? "null";

                templates.Add(TextInputTemplate
                    .Create(parameter.Name)
                    .WithLabel(parameter.Name)
                    .WithDefaultText(text)
                    .WithPlaceholder(text));
            }

            templates.Add(ButtonTemplate
                .Create("ASS.Collections Builder Build Button")
                .WithLabel("Create")
                .WithHoldTime(HoldTime)
                .WithHandler((player, button) =>
                {
                    // it should be noted here all the index math just needs to ignore the first entry in templates. This includes this entry though as this is the buttons HANDLER.
                    object?[] args = new object?[templates.Count - 2];

                    for (int i = 1; i < templates.Count - 1; i++)
                    {
                        if (templates[i] is not TextInputTemplate textInput)
                        {
                            Logger.Error("Invalid Builder state, found a non-text input!");

                            foreach (ASSTemplate template in templates)
                            {
                                Logger.Warn(template);
                            }

                            return;
                        }

                        if (!textInput.Instances.TryGetValue(player, out TextInputInstance? input))
                            return;

                        Type targetType = parameters[i - 1].ParameterType;
                        if (!Parsers.TryGetValue(targetType, out Func<string, BindingHelper.ParserResponse> parser) && !BindingHelper.GlobalParsers.TryGetValue(targetType, out parser))
                        {
                            if (!SuppressLogs)
                                Logger.Warn($"Cannot build type {Constructor.DeclaringType?.FullName ?? "null"} as no parser for {targetType.FullName} was found");
                            return;
                        }

                        BindingHelper.ParserResponse response = parser(input.InputtedText);
                        if (response.IsError)
                            return;

                        args[i - 1] = response.Value;
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

            return templates;
        }

        private void RebuildParameters()
        {
            parameters.Clear();
            templates.Clear();

            if (Constructor is null)
                return;

            parameters.AddRange(Constructor.GetParameters());
        }
    }
}