namespace ASS.Collections.Features
{
    extern alias oldSystem;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using ASS.Collections.Features.Helpers;

    using ASS.Features;
    using ASS.Features.Attributes;
    using ASS.Features.Helpers;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Parsers;
    using ASS.Features.Templates;

    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;

    using oldSystem::System.Collections.ObjectModel;
    using oldSystem::System.ComponentModel;

    /// <summary>
    /// A complex class that essentially converts every property, field, or method of a target object into a TextInput or Button.
    /// </summary>
    [Fluent]
    public partial class Binding : IASSContainer
    {
        private readonly Dictionary<string, MemberInfo> members = new();
        private readonly List<IASSObject> children = [];

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

        public string Key { get; }

        [Fluent]
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

        [Fluent]
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

        [Fluent]
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

        [Fluent]
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

        [Fluent]
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

        [Fluent]
        public bool HandleSets { get; set; }

        [Fluent]
        public bool UseClipboard { get; set; }

        [Fluent(false)]
        public bool IgnoreDefaultResponses { get; set; } = true;

        [Fluent(false)]
        public bool SuppressLogs { get; set; } = true;

        public ObservableCollection<string> Blacklist { get; } = [];

        public ObservableCollection<string> Whitelist { get; } = [];

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public static Binding Create(string key) => new(key);

        public Binding WithBlacklist(params string[] blacklist)
        {
            for (int i = 0; i < blacklist.Length; i++)
            {
                Blacklist.Add(blacklist[i]);
            }

            return this;
        }

        public Binding WithWhitelist(params string[] whitelist)
        {
            for (int i = 0; i < whitelist.Length; i++)
            {
                Whitelist.Add(whitelist[i]);
            }

            return this;
        }

        public IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            // re-use old templates unless rebuilt in RebuildMembers.
            if (children.Count > 0)
                return children;

            if (Target is null)
                return [];

            foreach (MemberInfo member in members.Values)
            {
                switch (member)
                {
                    case FieldInfo field:
                    {
                        Action<object, object?> setMethod = field.IsInitOnly
                            ? BindingHelper.CreateRefSetter(field)
                            : field.SetValue;

                        AddEditor(field, field.FieldType, setMethod, field.GetValue(Target));
                        break;
                    }

                    case PropertyInfo property:
                    {
                        if (!property.CanWrite)
                            break;

                        object? value = null;
                        if (property.CanRead)
                            value = property.GetMethod.Invoke(Target, null);

                        AddEditor(property, property.PropertyType, property.SetValue, value);
                        break;
                    }

                    case MethodInfo method:
                        AddMethod(method);
                        break;
                }
            }

            return children;

            void AddMethod(MethodInfo method)
            {
                if (Target is null)
                    return;

                children.Add(ButtonTemplate
                    .Create(method.Name)
                    .WithHandler((player, input) => method.Invoke(Target, null))
                    .WithLabel($"{method.Name}()"));
            }

            void AddEditor(MemberInfo member, Type type, Action<object, object?> setValue, object? currentValue)
            {
                if (Target is null)
                    return;

                DescriptionAttribute? attr = member.GetCustomAttribute<DescriptionAttribute>();

                if (HandleSets && type.IsSet())
                {
                    children.Add(HeaderTemplate
                        .Create($"{member.Name} SetEditor Header")
                        .WithLabel($"{member.Name} Editor: [{type.ProperTypeName()}]")
                        .WithReducedPadding());

                    children.Add(SetEditor
                        .Create(member.Name)
                        .WithSet(currentValue as IEnumerable, array => setValue.Invoke(Target, array)));

                    return;
                }

                if (!Clipboard.TryGetParser(type, out ParserTemplate parserTemplate))
                {
                    if (!SuppressLogs)
                        Logger.Warn($"Cannot bind type {Target.GetType().FullName} as no parser for {type.FullName} was found");

                    return;
                }

                ASSParser parser = parserTemplate.Instantiate(member.Name);
                parser.Transform(member.GetCustomAttributes().OfType<IParserTransform>());

                if (parser is IHeaderParser header)
                {
                    header.Header
                        .WithLabel($"{member.Name}: ({parserTemplate.Print(currentValue)}) [{type.ProperTypeName()}]")
                        .WithReducedPadding();

                    if (attr is not null)
                        header.Header.WithHint(attr.Description);
                }

                children.Add(parser);

                parser.OnModified = player =>
                {
                    if (!parser.TryParse(player, out object? value))
                        return;

                    setValue.Invoke(Target, value);

                    if (parser is IHeaderParser header2)
                    {
                        string text = $"{member.Name}: ({parserTemplate.Print(value)}) [{type.ProperTypeName()}]";
                        header2.Header.Label = text;
                        if (header2.Header.Instances.TryGetValue(player, out HeaderInstance instance))
                            instance.Label = text;
                    }
                };

                if (UseClipboard && Clipboard.TryGetValue(viewer, out object? obj) && obj.GetType() == type)
                {
                    children.Add(ButtonTemplate
                        .Create(member.Name + "Clipboard Button")
                        .WithHandler((player, button) => setValue.Invoke(Target, obj))
                        .WithLabel($"Load from Clipboard {obj} to {member.Name}"));
                }
            }
        }

        private void RebuildMembers()
        {
            members.Clear();
            children.Clear();

            if (Target is null)
                return;

            Type type = Target.GetType();

            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            if (NonPublic)
                flags |= BindingFlags.NonPublic;

            foreach (MemberInfo member in type.GetMembers(flags).OrderBy(m => type == m.DeclaringType ? 1 : -1))
            {
                if (Whitelist.Count > 0 && !Whitelist.Contains(member.Name))
                    continue;

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