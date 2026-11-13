namespace ASS.Collections.Features
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using ASS.Collections.Features.Helpers;
    using ASS.Collections.Features.Pages;

    using ASS.Features;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Logger = LabApi.Features.Console.Logger;

    /// <summary>
    /// A generalized editor for <see cref="HashSet{T}"/>s, <see cref="List{T}"/>s, and <see cref="Array"/>s.
    /// </summary>
    /// <remarks>Manual modifications to <see cref="Set"/> should update viewers of this container.</remarks>
    public class SetEditor : IASSContainer
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
        private Func<string, BindingHelper.ParserResponse>? parser;
        private Action<Array>? onResize;

        private SetEditor(string key)
        {
            Key = key;

            smartGenerator = SmartGenerator.Create("ASS.Collections SetEditor Sub-Menu SmartGenerator", Generator, Validator);

            buttonPage.WithChildren(smartGenerator);
        }

        private enum SetType
        {
            HashSet = 0,
            List = 1,
            Array = 2,
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
                    parser = null;
                    return;
                }

                type = value.GetType();

                if (type.IsArray)
                {
                    setType = SetType.Array;
                    return;
                }

                if (!type.IsConstructedGenericType)
                    throw new InvalidOperationException("SetEditors can only be used on HashSets, Lists, and Arrays!");

                Type definition = type.GetGenericTypeDefinition();
                if (definition == typeof(HashSet<>))
                    setType = SetType.HashSet;
                else if (definition == typeof(List<>))
                    setType = SetType.List;
                else
                    throw new InvalidOperationException("SetEditors can only be used on HashSets, Lists, and Arrays!");

                targetType = type.GenericTypeArguments[0];

                if (!BindingHelper.GlobalParsers.TryGetValue(targetType, out parser))
                {
                    parser = null;
                    Logger.Warn($"Cannot create SetEditor for type {targetType.FullName}, there is no parser available!");
                }
            }
        }

        public float HoldTime { get; set; }

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public static SetEditor Create(string key) => new(key);

        /// <summary>
        /// The standard way to use a <see cref="SetEditor"/>. <see cref="Array"/>'s need special attention.
        /// </summary>
        /// <param name="set">The set to modify.</param>
        /// <param name="onResize">An action triggered whenever Set is an array, and a resize occurs.</param>
        /// <returns>The current instance.</returns>
        /// <remarks>If you are using an Array, set onResize to reset the original set.</remarks>
        public SetEditor WithSet(IEnumerable? set, Action<Array>? onResize = null)
        {
            Set = set;
            this.onResize = onResize;

            return this;
        }

        public SetEditor WithHoldTime(float holdTime)
        {
            HoldTime = holdTime;
            return this;
        }

        public IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            if (type is null || targetType is null || Set is null || parser is null)
                yield break;

            list.Clear();
            smartGenerator.InvalidateAll();

            foreach (object? obj in Set)
                list.Add(obj);

            yield return DropdownTemplate
                .Create("Options Dropdown")
                .WithLabel($"Current Values ({BindingHelper.ProperTypeName(type)})")
                .WithOptions(list
                    .Select(obj => obj?.ToString() ?? NullKeyword)
                    .Append($"Create New {BindingHelper.ProperTypeName(targetType)}")
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
                    }

                    ASSNetworking.SetDirty(player);
                });

            yield return buttonPage;
        }

        private bool Validator(Player player, object? state) => selections.TryGetValue(player, out (object? Selected, int Index) pair) && Equals(pair.Index, state);

        private IEnumerable<IASSObject> Generator(SmartGenerator generator, Player p)
        {
            if (Set is null || type is null || targetType is null || parser is null)
                yield break;

            if (!selections.TryGetValue(p, out (object? Selected, int Index) pair))
                yield break;

            generator.SetState(p, pair.Index);

            bool isNew = pair.Index == list.Count;

            if (isNew)
            {
                yield return HeaderTemplate
                    .Create("Header")
                    .WithLabel($"Create new {BindingHelper.ProperTypeName(targetType)}");
            }
            else
            {
                yield return HeaderTemplate
                    .Create("Header")
                    .WithLabel($"Modify {pair.Selected?.ToString() ?? NullKeyword} ({BindingHelper.ProperTypeName(targetType)})");
            }

            switch (setType)
            {
                case SetType.HashSet:
                    if (isNew)
                    {
                        TextInputTemplate textInput = TextInputTemplate
                            .Create("Creator Text Input")
                            .WithLabel($"Create New {BindingHelper.ProperTypeName(targetType)}");

                        yield return textInput;

                        yield return ButtonTemplate
                            .Create("Creator Button")
                            .WithLabel("Create")
                            .WithHandler((player, button) =>
                            {
                                if (!textInput.Instances.TryGetValue(player, out TextInputInstance? input))
                                    return;

                                BindingHelper.ParserResponse response = parser(input.InputtedText);

                                if (response.IsError)
                                    return;

                                SetEditorHelper.GetICollectionAdder(targetType).Invoke(Set, [response.Value]);

                                buttonPage.Exit(player);
                            })
                            .WithHoldTime(HoldTime);
                    }
                    else
                    {
                        yield return ButtonTemplate
                            .Create("Remover Button")
                            .WithLabel($"Remove {pair.Selected?.ToString() ?? NullKeyword} ({BindingHelper.ProperTypeName(targetType)})")
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
                        TextInputTemplate textInput = TextInputTemplate
                            .Create("Creator Text Input")
                            .WithLabel($"Create New {BindingHelper.ProperTypeName(targetType)}");

                        yield return textInput;

                        yield return ButtonTemplate
                            .Create("Creator Button")
                            .WithLabel("Create")
                            .WithHandler((player, button) =>
                            {
                                if (!textInput.Instances.TryGetValue(player, out TextInputInstance? input))
                                    return;

                                BindingHelper.ParserResponse response = parser(input.InputtedText);

                                if (response.IsError)
                                    return;

                                if (Set is Array array)
                                {
                                    object[] args = [Set, array.Length + 1];
                                    SetEditorHelper.GetResizer(targetType).Invoke(null, args);
                                    Set = array = (Array)args[0];

                                    array.SetValue(response.Value, pair.Index);
                                    onResize?.Invoke(array);
                                }
                                else
                                {
                                    SetEditorHelper.GetICollectionAdder(targetType).Invoke(Set, [response.Value]);
                                }

                                buttonPage.Exit(player);
                            })
                            .WithHoldTime(HoldTime);
                    }
                    else
                    {
                        yield return TextInputTemplate
                            .Create("Modifier Text Input")
                            .WithLabel($"Modify {pair.Selected?.ToString() ?? NullKeyword} ({BindingHelper.ProperTypeName(targetType)})")
                            .WithHandler((player, input) =>
                            {
                                if (input.IsInitialResponse)
                                    return;

                                BindingHelper.ParserResponse response = parser(input.InputtedText);

                                if (response.IsError)
                                    return;

                                if (Set is Array array)
                                    array.SetValue(response.Value, pair.Index);
                                else
                                    SetEditorHelper.GetListSetItem(targetType).Invoke(Set, [pair.Index, response.Value]);

                                buttonPage.Exit(player);
                            });

                        yield return ButtonTemplate
                            .Create("Remover Button")
                            .WithLabel($"Remove {pair.Selected?.ToString() ?? NullKeyword} ({BindingHelper.ProperTypeName(targetType)})")
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}