namespace ASS.Collections.Features
{
    extern alias oldSystem;

    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using ASS.Features.Helpers;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;

    /// <summary>
    /// A complex class that essentially converts every property, field, or method of a target object into a TextInput or Button.
    /// </summary>
    /// <remarks>For custom parsers, see <see cref="BindingHelper.GlobalParsers"/> or <see cref="Parsers"/>.</remarks>
    public class Binding : IASSContainer
    {
        private readonly Dictionary<string, MemberInfo> members = new();
        private readonly List<ASSTemplate> templates = [];

        private Binding(string key)
        {
            Key = key;

            Blacklist.CollectionChanged += (sender, args) =>
            {
                if (args.Action is oldSystem::System.Collections.Specialized.NotifyCollectionChangedAction.Move)
                    return;

                RebuildMembers();
            };
        }

        public Dictionary<Type, Func<string, BindingHelper.ParserResponse>> Parsers { get; } = new();

        public string Key { get; }

        public object? Target
        {
            get;
            set
            {
                bool rebuild = field?.GetType() != value?.GetType();

                field = value;

                if (rebuild)
                    RebuildMembers();
            }
        }

        public bool Properties
        {
            get;
            set
            {
                bool change = field ^ value;

                field = value;

                if (change)
                    RebuildMembers();
            }
        }

        public bool Fields
        {
            get;
            set
            {
                bool change = field ^ value;

                field = value;

                if (change)
                    RebuildMembers();
            }
        }

        public bool Methods
        {
            get;
            set
            {
                bool change = field ^ value;

                field = value;

                if (change)
                    RebuildMembers();
            }
        }

        public bool NonPublic
        {
            get;
            set
            {
                bool change = field ^ value;

                field = value;

                if (change)
                    RebuildMembers();
            }
        }

        public bool IgnoreDefaultResponses { get; set; } = true;

        public bool SuppressLogs { get; set; } = true;

        public oldSystem::System.Collections.ObjectModel.ObservableCollection<string> Blacklist { get; } = [];

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public static Binding Create(string key) => new(key);

        public Binding WithTarget(object? target)
        {
            Target = target;
            return this;
        }

        public Binding WithProperties(bool properties = true)
        {
            Properties = properties;
            return this;
        }

        public Binding WithFields(bool fields = true)
        {
            Fields = fields;
            return this;
        }

        public Binding WithMethods(bool methods = true)
        {
            Methods = methods;
            return this;
        }

        public Binding WithNonPublic(bool nonPublic = true)
        {
            NonPublic = nonPublic;
            return this;
        }

        public Binding WithBlacklist(params string[] blacklist)
        {
            for (int i = 0; i < blacklist.Length; i++)
            {
                Blacklist.Add(blacklist[i]);
            }

            return this;
        }

        public Binding WithoutIgnoreDefaultResponses(bool ignoreDefaultResponses = false)
        {
            IgnoreDefaultResponses = ignoreDefaultResponses;
            return this;
        }

        public Binding WithoutSuppressLogs(bool suppressLogs = false)
        {
            SuppressLogs = suppressLogs;
            return this;
        }

        public IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            // re-use old templates unless rebuilt in RebuildMembers.
            if (templates.Count > 0)
                return templates;

            if (Target is null)
                return [];

            foreach (MemberInfo member in members.Values)
            {
                switch (member)
                {
                    case FieldInfo field:
                        Action<object, object?> setMethod = field.IsInitOnly
                            ? BindingHelper.CreateRefSetter(field)
                            : field.SetValue;

                        AddTextInput(field, field.FieldType, setMethod, field.GetValue(Target));
                        break;
                    case PropertyInfo property:
                        if (!property.CanWrite)
                            break;

                        object? value = null;
                        if (property.CanRead)
                            value = property.GetMethod.Invoke(Target, null);

                        AddTextInput(property, property.PropertyType, property.SetValue, value);
                        break;
                    case MethodInfo method:
                        AddMethod(method);
                        break;
                }
            }

            return templates;

            void AddMethod(MethodInfo method)
            {
                if (Target is null)
                    return;

                templates.Add(ButtonTemplate
                    .Create(method.Name)
                    .WithHandler((player, input) => method.Invoke(Target, null))
                    .WithLabel($"{method.Name}()"));
            }

            void AddTextInput(MemberInfo member, Type type, Action<object, object?> setValue, object? currentValue)
            {
                if (Target is null)
                    return;

                if (!Parsers.TryGetValue(type, out Func<string, BindingHelper.ParserResponse> parser) && !BindingHelper.GlobalParsers.TryGetValue(type, out parser))
                {
                    if (type.IsEnum)
                    {
                        BindingHelper.GlobalParsers[type] = parser = BindingHelper.CreateParserForEnum(type);
                    }
                    else
                    {
                        if (!SuppressLogs)
                            Logger.Warn($"Cannot bind type {Target.GetType().FullName} as no parser for {type.FullName} was found");

                        return;
                    }
                }

                string text = currentValue?.ToString() ?? "null";
                templates.Add(TextInputTemplate
                    .Create(member.Name)
                    .WithDefaultText(text)
                    .WithPlaceholder(text)
                    .WithHandler((player, input) =>
                    {
                        if (IgnoreDefaultResponses && input.IsInitialResponse)
                            return;

                        BindingHelper.ParserResponse response = parser(input.InputtedText);
                        if (response.IsError)
                            return;

                        string text = response.Value?.ToString() ?? "null";
                        input.DefaultText = text;
                        input.Placeholder = text;

                        setValue.Invoke(Target, response.Value);
                    })
                    .WithLabel($"{member.Name}: ({BindingHelper.ProperTypeName(type)})"));
            }
        }

        private void RebuildMembers()
        {
            members.Clear();
            templates.Clear();

            if (Target is null)
                return;

            Type type = Target.GetType();

            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            if (NonPublic)
                flags |= BindingFlags.NonPublic;

            foreach (MemberInfo member in type.GetMembers(flags))
            {
                if (Blacklist.Contains(member.Name))
                    continue;
                switch (member)
                {
                    case MethodInfo method when Methods && method.GetParameters().Length is 0 && !method.Name.StartsWith("get_"):
                    case FieldInfo when Fields:
                    case PropertyInfo when Properties:
                        members[member.Name] = member;
                        break;
                }
            }
        }
    }
}