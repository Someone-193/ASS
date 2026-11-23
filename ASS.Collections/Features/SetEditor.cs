namespace ASS.Collections.Features
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using ASS.Collections.Features.Helpers;
    using ASS.Collections.Features.Pages;

    using ASS.Features;
    using ASS.Features.Attributes;
    using ASS.Features.Helpers;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Parsers;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Logger = LabApi.Features.Console.Logger;

    /// <summary>
    /// A generalized editor for <see cref="HashSet{T}"/>s, <see cref="List{T}"/>s, <see cref="Array"/>s, and <see cref="Dictionary{TKey,TValue}"/>s.
    /// </summary>
    /// <remarks>Manual modifications to <see cref="Set"/> should update viewers of this container.</remarks>
    [Fluent]

    // this kids, is why YOU DONT CRAM 1000 LINES OF CODE IN A SINGLE FILE AHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH
    // I'm being fr, if you want to try and fix this somehow, just ping me on discord or something. This thing needs a full rewrite. (But at least it works, and it's pretty cool!)
    public partial class SetEditor : IASSContainer
    {
        private const string NullKeyword = "<color=#6C95EB>null</color>";

        private readonly Dictionary<Player, (object?, int)> selections = new();

        private readonly List<object?> list = [];

        private readonly SmartGenerator smartGenerator;
        private readonly ButtonPage buttonPage = ButtonPage
            .Create("Modify Button")
            .WithButtonText("Modify", "Exit");

        private SetType setType;
        private Type? type;
        private Type? targetType;
        private Type? dictKey;
        private ParserTemplate parserTemplate;
        private ParserTemplate dictKeyTemplate;
        private Action<Array>? onResize;

        private SetEditor(string key)
        {
            Key = key;

            smartGenerator = SmartGenerator.Create("ASS.Collections SetEditor Sub-Menu SmartGenerator", Generator, Validator);

            buttonPage.Parent = this;
            buttonPage.WithChildren(smartGenerator);
        }

        private enum SetType
        {
            HashSet = 0,
            List = 1,
            Array = 2,
            Dictionary = 3,
        }

        public string Key { get; }

        public IEnumerable? Set
        {
            get;
            set
            {
                field = value;

                if (value is null)
                {
                    type = null;
                    targetType = null;
                    parserTemplate = default;
                    return;
                }

                type = value.GetType();

                if (type.IsArray)
                {
                    setType = SetType.Array;
                    return;
                }

                if (!type.IsConstructedGenericType)
                    throw new InvalidOperationException("SetEditors can only be used on HashSets, Lists, and Arrays, Dictionaries!");

                Type definition = type.GetGenericTypeDefinition();
                if (definition == typeof(HashSet<>))
                {
                    targetType = type.GenericTypeArguments[0];
                    setType = SetType.HashSet;
                }
                else if (definition == typeof(List<>))
                {
                    targetType = type.GenericTypeArguments[0];
                    setType = SetType.List;
                }
                else if (definition == typeof(Dictionary<,>))
                {
                    targetType = type.GenericTypeArguments[1];
                    setType = SetType.Dictionary;
                    dictKey = type.GenericTypeArguments[0];

                    if (!Clipboard.TryGetParser(dictKey, out dictKeyTemplate))
                    {
                        dictKeyTemplate = default;
                        Logger.Warn($"Cannot create SetEditor for type {dictKey.FullName}, there is no parser available!");
                    }
                }
                else
                    throw new InvalidOperationException("SetEditors can only be used on HashSets, Lists, and Arrays, Dictionaries!");

                if (!Clipboard.TryGetParser(targetType, out parserTemplate))
                {
                    parserTemplate = default;
                    Logger.Warn($"Cannot create SetEditor for type {targetType.FullName}, there is no parser available!");
                }
            }
        }

        [Fluent]
        public float HoldTime { get; set; }

        [Fluent]
        public bool UseClipboard { get; set; }

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public static SetEditor Create(string key) => new(key);

        /// <summary>
        /// The standard way to use a <see cref="SetEditor"/>. <see cref="Array"/>'s need special attention.
        /// </summary>
        /// <param name="set">The set to modify.</param>
        /// <param name="onResize">An action triggered whenever Set is an array, and the array is resized.</param>
        /// <returns>The current instance.</returns>
        /// <remarks>If you are using an Array, set onResize to reset the source array.</remarks>
        public SetEditor WithSet(IEnumerable? set, Action<Array>? onResize = null)
        {
            Set = set;
            this.onResize = onResize;

            return this;
        }

        public IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            if (type is null || targetType is null || Set is null || parserTemplate == default)
                yield break;

            if (setType is SetType.Dictionary && dictKey is null)
                yield break;

            list.Clear();
            smartGenerator.InvalidateAll();

            foreach (object? obj in Set)
                list.Add(obj);

            yield return DropdownTemplate
                .Create("Options Dropdown")
                .WithLabel($"Current Values ({type.ProperTypeName()})")
                .WithOptions(list
                    .Select(obj => parserTemplate.Print(obj))
                    .Append($"Create New {targetType.ProperTypeName()}")
                    .ToArray())
                .WithHandler((player, dropdown) =>
                {
                    object? obj = dropdown.IndexSelected == dropdown.Options.Length - 1
                        ? null
                        : list[dropdown.IndexSelected];

                    selections[player] = (obj, dropdown.IndexSelected);
                });

            yield return ButtonTemplate
                .Create("Clear Button")
                .WithLabel("Clear")
                .WithHandler((player, button) =>
                {
                    switch (setType)
                    {
                        case SetType.HashSet:
                        case SetType.List:
                            SetEditorHelper.GetICollectionClear(targetType).Invoke(Set, null);
                            break;
                        case SetType.Array:
                            Array array = (Array)SetEditorHelper.GetEmpty(targetType).Invoke(null, null);
                            Set = array;

                            onResize?.Invoke(array);
                            break;
                        case SetType.Dictionary:
                            SetEditorHelper.GetDictionaryClear(dictKey!, targetType).Invoke(Set, null);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    ASSNetworking.SetDirty(player);
                })
                .WithHoldTime(HoldTime);

            yield return buttonPage;
        }

        private bool Validator(Player player, object? state) => selections.TryGetValue(player, out (object? Selected, int Index) pair) && Equals(pair.Index, state);

        private IEnumerable<IASSObject> Generator(SmartGenerator generator, Player p)
        {
            if (Set is null || type is null || targetType is null || parserTemplate == default)
                yield break;

            if (setType is SetType.Dictionary && dictKey is null)
                yield break;

            if (!selections.TryGetValue(p, out (object? Selected, int Index) pair))
                yield break;

            generator.SetState(p, pair.Index);

            bool isNew = pair.Index == list.Count;

            if (isNew)
            {
                yield return HeaderTemplate
                    .Create("Header")
                    .WithLabel($"Create new {targetType.ProperTypeName()}");
            }
            else
            {
                yield return HeaderTemplate
                    .Create("Header")
                    .WithLabel($"Modify Value: ({parserTemplate.Print(pair.Selected)}) [{targetType.ProperTypeName()}]");
            }

            ASSParser parser = parserTemplate.Instantiate("SetEditor Parser");

            switch (setType)
            {
                case SetType.HashSet:
                    if (isNew)
                    {
                        if (parser is IHeaderParser headerParser)
                        {
                            headerParser.Header
                                .WithLabel($"Create New {targetType.ProperTypeName()}")
                                .WithReducedPadding();
                        }

                        yield return parser;

                        if (UseClipboard && Clipboard.TryGetValue(p, out object? copied) && copied.GetType() == targetType)
                        {
                            yield return ButtonTemplate
                                .Create($"Create from Clipboard ({parserTemplate.Print(copied)})")
                                .WithHoldTime(HoldTime)
                                .WithHandler((player, button) =>
                                {
                                    SetEditorHelper.GetICollectionAdder(targetType).Invoke(Set, [copied]);

                                    buttonPage.Exit(player);
                                });
                        }

                        yield return ButtonTemplate
                            .Create("Creator Button")
                            .WithLabel("Create")
                            .WithHandler((player, button) =>
                            {
                                if (!parser.TryParse(player, out object? obj))
                                    return;

                                SetEditorHelper.GetICollectionAdder(targetType).Invoke(Set, [obj]);

                                buttonPage.Exit(player);
                            })
                            .WithHoldTime(HoldTime);
                    }
                    else
                    {
                        yield return ButtonTemplate
                            .Create("Remover Button")
                            .WithLabel($"Remove ({parserTemplate.Print(pair.Selected)}) [{targetType.ProperTypeName()}]")
                            .WithHandler((player, button) =>
                            {
                                SetEditorHelper.GetICollectionRemover(targetType).Invoke(Set, [pair.Selected]);

                                buttonPage.Exit(player);
                            })
                            .WithHoldTime(HoldTime);
                    }

                    break;
                case SetType.List:
                case SetType.Array:
                    if (isNew)
                    {
                        if (parser is IHeaderParser headerParser)
                        {
                            headerParser.Header
                                .WithLabel($"Create New {targetType.ProperTypeName()}")
                                .WithReducedPadding();
                        }

                        yield return parser;

                        if (UseClipboard && Clipboard.TryGetValue(p, out object? copied) && copied.GetType() == targetType)
                        {
                            yield return ButtonTemplate
                                .Create($"Create from Clipboard ({parserTemplate.Print(copied)})")
                                .WithHoldTime(HoldTime)
                                .WithHandler((player, button) =>
                                {
                                    if (Set is Array array)
                                    {
                                        object[] args = [Set, array.Length + 1];
                                        SetEditorHelper.GetResizer(targetType).Invoke(null, args);
                                        Set = array = (Array)args[0];

                                        array.SetValue(copied, pair.Index);
                                        onResize?.Invoke(array);
                                    }
                                    else
                                    {
                                        SetEditorHelper.GetICollectionAdder(targetType).Invoke(Set, [copied]);
                                    }

                                    buttonPage.Exit(player);
                                });
                        }

                        yield return ButtonTemplate
                            .Create("Creator Button")
                            .WithLabel("Create")
                            .WithHandler((player, button) =>
                            {
                                if (!parser.TryParse(player, out object? obj))
                                    return;

                                if (Set is Array array)
                                {
                                    object[] args = [Set, array.Length + 1];
                                    SetEditorHelper.GetResizer(targetType).Invoke(null, args);
                                    Set = array = (Array)args[0];

                                    array.SetValue(obj, pair.Index);
                                    onResize?.Invoke(array);
                                }
                                else
                                {
                                    SetEditorHelper.GetICollectionAdder(targetType).Invoke(Set, [obj]);
                                }

                                buttonPage.Exit(player);
                            })
                            .WithHoldTime(HoldTime);
                    }
                    else
                    {
                        if (parser is IHeaderParser headerParser)
                        {
                            headerParser.Header
                                .WithLabel($"Modify Value: ({parserTemplate.Print(pair.Selected)}) [{targetType.ProperTypeName()}]")
                                .WithReducedPadding();
                        }

                        parser.OnModified = player =>
                            {
                                if (!parser.TryParse(player, out object? obj))
                                    return;

                                if (Set is Array array)
                                    array.SetValue(obj, pair.Index);
                                else
                                    SetEditorHelper.GetListSetItem(targetType).Invoke(Set, [pair.Index, obj]);

                                if (parser is not IHeaderParser header)
                                    return;

                                if (header.Header.Instances.TryGetValue(player, out HeaderInstance instance))
                                    instance.Label = $"Modify Value: ({parserTemplate.Print(pair.Selected)}) [{targetType.ProperTypeName()}]";
                            };

                        if (UseClipboard && Clipboard.TryGetValue(p, out object? copied) && copied.GetType() == targetType)
                        {
                            yield return ButtonTemplate
                                .Create($"Load from Clipboard ({parserTemplate.Print(copied)})")
                                .WithHoldTime(HoldTime)
                                .WithHandler((player, button) =>
                                {
                                    if (Set is Array array)
                                        array.SetValue(copied, pair.Index);
                                    else
                                        SetEditorHelper.GetListSetItem(targetType).Invoke(Set, [pair.Index, copied]);

                                    buttonPage.Exit(player);
                                });
                        }

                        yield return ButtonTemplate
                            .Create("Remover Button")
                            .WithLabel($"Remove ({parserTemplate.Print(pair.Selected)}) [{targetType.ProperTypeName()}]")
                            .WithHandler((player, button) =>
                            {
                                if (Set is Array array)
                                {
                                    // [1, 2, 3, 4] RemoveAt(1)
                                    // sourceIdx = [2] (3);
                                    // destIdx = [1] (2);
                                    // len = arrLen [4] - destIdx [1] - 1 = 2;
                                    // -> [1, 3, 4].
                                    Array.Copy(array, pair.Index + 1, array, pair.Index, array.Length - pair.Index - 1);

                                    object[] args = [Set, array.Length - 1];
                                    SetEditorHelper.GetResizer(targetType).Invoke(null, args);
                                    Set = array = (Array)args[0];

                                    onResize?.Invoke(array);
                                }
                                else
                                {
                                    SetEditorHelper.GetListRemoveAt(targetType).Invoke(Set, [pair.Index]);
                                }

                                buttonPage.Exit(player);
                            })
                            .WithHoldTime(HoldTime);
                    }

                    break;
                case SetType.Dictionary:
                    if (isNew)
                    {
                        ASSParser keyParser = dictKeyTemplate.Instantiate("SetEditor Dictionary Key Parser");

                        if (keyParser is IHeaderParser keyHeader)
                        {
                            keyHeader.Header
                                .WithLabel($"Create Key ({dictKey!.ProperTypeName()})")
                                .WithReducedPadding();
                        }

                        yield return keyParser;

                        if (parser is IHeaderParser valueHeader)
                        {
                            valueHeader.Header
                                .WithLabel($"Create Value ({targetType.ProperTypeName()})")
                                .WithReducedPadding();
                        }

                        yield return parser;

                        yield return ButtonTemplate
                            .Create("Creator Button")
                            .WithLabel("Create")
                            .WithHandler((player, button) =>
                            {
                                if (!keyParser.TryParse(player, out object? key))
                                    return;

                                if (SetEditorHelper.GetDictionaryContainsKey(dictKey!, targetType).Invoke(Set, [key]) is true)
                                    return;

                                if (!parser.TryParse(player, out object? value))
                                    return;

                                SetEditorHelper.GetDictionaryAdd(dictKey!, targetType).Invoke(Set, [key, value]);
                                buttonPage.Exit(player);
                            })
                            .WithHoldTime(HoldTime);
                    }
                    else
                    {
                        if (parser is IHeaderParser headerParser)
                        {
                            headerParser.Header
                                .WithLabel($"Modify Value: ({parserTemplate.Print(pair.Selected)}) [{targetType.ProperTypeName()}]")
                                .WithReducedPadding();
                        }

                        parser.OnModified = player =>
                            {
                                if (!parser.TryParse(player, out object? value))
                                    return;

                                object key = SetEditorHelper.GetKeyFromKvp(dictKey!, targetType).Invoke(pair.Selected, null);

                                SetEditorHelper.GetDictionarySetItem(dictKey!, targetType).Invoke(Set, [key, value]);

                                if (parser is not IHeaderParser header)
                                    return;

                                if (header.Header.Instances.TryGetValue(player, out HeaderInstance instance))
                                    instance.Label = $"Modify Value: ({parserTemplate.Print(pair.Selected)}) [{targetType.ProperTypeName()}]";
                            };

                        if (UseClipboard && Clipboard.TryGetValue(p, out object? copied) && copied.GetType() == targetType)
                        {
                            yield return ButtonTemplate
                                .Create($"Load from Clipboard ({parserTemplate.Print(copied)})")
                                .WithHoldTime(HoldTime)
                                .WithHandler((player, button) =>
                                {
                                    object key = SetEditorHelper.GetKeyFromKvp(dictKey!, targetType).Invoke(pair.Selected, null);

                                    SetEditorHelper.GetDictionarySetItem(dictKey!, targetType).Invoke(Set, [key, copied]);

                                    buttonPage.Exit(player);
                                });
                        }

                        yield return ButtonTemplate
                            .Create("Remover Button")
                            .WithLabel($"Remove ({parserTemplate.Print(pair.Selected)}) [{targetType.ProperTypeName()}]")
                            .WithHandler((player, button) =>
                            {
                                object key = SetEditorHelper.GetKeyFromKvp(dictKey!, targetType).Invoke(pair.Selected, null);

                                SetEditorHelper.GetDictionaryRemove(dictKey!, targetType).Invoke(Set, [key]);

                                buttonPage.Exit(player);
                            })
                            .WithHoldTime(HoldTime);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}