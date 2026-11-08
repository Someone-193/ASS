namespace ASS.Collections.Features
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using ASS.Collections.Features.Pages;
    using ASS.Features.Helpers;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Logger = LabApi.Features.Console.Logger;

    /// <summary>
    /// A generalized editor for <see cref="HashSet{T}"/>s, <see cref="List{T}"/>s, and <see cref="Array"/>s.
    /// </summary>
    public class SetEditor : IASSContainer
    {
        private readonly Dictionary<Player, (object, int)> selections = new();

        private readonly List<object> list = [];

        private readonly SmartGenerator smartGenerator;

        private SetType setType;
        private Type? targetType;

        private SetEditor(string key)
        {
            Key = key;

            smartGenerator = SmartGenerator.Create("ASS.Collections SetEditor Sub-Menu SmartGenerator", Generator, Validator);
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
                if (value is not null)
                {
                    Type type = value.GetType();

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
                }
            }
        }

        public float HoldTime { get; set; }

        public IASSContainer? Parent { get; set; }

        public bool DisplayInHierarchy { get; set; } = true;

        public static SetEditor Create(string key) => new(key);

        public SetEditor WithSet(IEnumerable set)
        {
            Set = set;
            return this;
        }

        public SetEditor WithHoldTime(float holdTime)
        {
            HoldTime = holdTime;
            return this;
        }

        public IEnumerable<IASSObject> GetChildren(Player viewer)
        {
            if (targetType is null || Set is null)
                yield break;

            if (!BindingHelper.GlobalParsers.TryGetValue(targetType, out Func<string, BindingHelper.ParserResponse>? parser))
            {
                Logger.Warn($"Cannot create SetEditor for type {targetType.FullName}, there is no parser available!");
                yield break;
            }

            foreach (object obj in Set)
                list.Add(obj);

            yield return DropdownTemplate
                .Create("ASS.Collections SetEditor Options Dropdown")
                .WithLabel("Current Values")
                .WithOptions(list
                    .Select(obj => obj.ToString())
                    .Append($"Create New {BindingHelper.ProperTypeName(targetType)}")
                    .ToArray())
                .WithHandler((player, dropdown) => selections[player] = (list[dropdown.IndexSelected], dropdown.IndexSelected));

            yield return ButtonPage
                .Create("ASS.Collections SetEditor Modify Button", smartGenerator)
                .WithLabel("Modify");
        }

        private bool Validator(Player player, object state) => selections.TryGetValue(player, out (object Selected, int Index) pair) && Equals(pair.Selected, state);

        private IEnumerable<IASSObject> Generator(SmartGenerator generator, Player player)
        {
            switch (setType)
            {
                case SetType.HashSet:
                    break;
                case SetType.List:
                    break;
                case SetType.Array:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // create some stuff based off setType and selections[player]. Make sure to account for Index for ordered sets.
            throw new NotImplementedException();
        }
    }
}