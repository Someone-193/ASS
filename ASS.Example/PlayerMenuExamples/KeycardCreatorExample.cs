namespace ASS.Example.PlayerMenuExamples
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using ASS.Features;
    using ASS.Features.Containers;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using Interactables.Interobjects.DoorUtils;

    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Features.Wrappers;

    using UnityEngine;

    // Idea yoinked from adyman0010 while trying to debug ASS :P
    public class KeycardCreatorExample
    {
        /// <summary>
        /// See ASSGroup Key in <see cref="Main.Enable"/>.
        /// </summary>
        public const string MenuRoot = "ASS.Example";
        public const string MenuKey = "Keycard Creator";

        public static PlayerMenu Menu { get; } = PlayerMenu.Create(MenuKey, Generator);

        private static Dictionary<Player, int> SelectedIndices { get; } = new();

        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            foreach (Player player in Menu.Children.Keys)
                Menu.Update(player, true, false);

            Menu.Add(ev.Player);
        }

        public static void OnLeft(PlayerLeftEventArgs ev)
        {
            Menu.Remove(ev.Player, false);

            foreach (Player player in Menu.Children.Keys)
                Menu.Update(player, true, false);
        }

        // you can always return a list, but this is technically more efficient, also still just as readable tbh
        private static IEnumerable<IASSObject> Generator(Player owner)
        {
            int index = 0;
            if (ASSNetworking.TryGetSetting(owner, [MenuRoot, MenuKey, "Type Selector"], out DropdownInstance? dropdown))
                index = dropdown.IndexSelected;

            yield return HeaderTemplate.Create("Header")
                .WithLabel("Keycard Creator");

            yield return DropdownTemplate.Create("Player Selector")
                .WithLabel("Player:")
                .WithOptions(Player.ReadyList.Select(p => p.Nickname).ToArray())
                .WithHint("The player who will receive the keycard");

            yield return DropdownTemplate.Create("Type Selector")
                .WithHandler(HandleTypeDropdown)
                .WithLabel("Keycard Type")
                .WithOptions([nameof(ItemType.KeycardCustomTaskForce), nameof(ItemType.KeycardCustomSite02), nameof(ItemType.KeycardCustomManagement), nameof(ItemType.KeycardCustomMetalCase)]);

            yield return TextInputTemplate.Create("Item Name")
                .WithLabel("Item Name")
                .WithPlaceholder("My Cool Keycard!")
                .WithCharacterLimit(ushort.MaxValue);

            yield return SliderTemplate.Create("Containment Level")
                .WithLabel("Containment Level")
                .WithDefaultValue(1)
                .WithMinValue(0)
                .WithMaxValue(3)
                .WithIsInteger();

            yield return SliderTemplate.Create("Armory Level")
                .WithLabel("Armory Level")
                .WithDefaultValue(2)
                .WithMinValue(0)
                .WithMaxValue(3)
                .WithIsInteger();

            yield return SliderTemplate.Create("Admin Level")
                .WithLabel("Admin Level")
                .WithDefaultValue(3)
                .WithMinValue(0)
                .WithMaxValue(3)
                .WithIsInteger();

            yield return TextInputTemplate.Create("Keycard Color")
                .WithLabel("Keycard Color (Hex)")
                .WithPlaceholder("#00FFFF")
                .WithCharacterLimit(7);

            yield return TextInputTemplate.Create("Keycard Permissions Color")
                .WithLabel("Keycard Permissions Color (Hex)")
                .WithPlaceholder("#FF0000")
                .WithCharacterLimit(7);

            switch (index)
            {
                case 0:
                    yield return TextInputTemplate.Create("Holder Name Input")
                        .WithLabel("Holder Name")
                        .WithDefaultText("A very cool person")
                        .WithCharacterLimit(ushort.MaxValue);

                    yield return TextInputTemplate.Create("Serial Label")
                        .WithLabel("Serial Label")
                        .WithDefaultText("69420")
                        .WithCharacterLimit(ushort.MaxValue);

                    yield return SliderTemplate.Create("Rank Index")
                        .WithLabel("Rank Index")
                        .WithDefaultValue(0)
                        .WithMinValue(0)
                        .WithMaxValue(3)
                        .WithIsInteger();
                    break;

                case 1:
                    yield return TextInputTemplate.Create("Holder Name Input")
                        .WithLabel("Holder Name")
                        .WithDefaultText("A very cool person")
                        .WithCharacterLimit(ushort.MaxValue);

                    yield return TextInputTemplate.Create("Keycard Label")
                        .WithLabel("Keycard Label")
                        .WithPlaceholder("Meth Card")
                        .WithCharacterLimit(ushort.MaxValue);

                    yield return TextInputTemplate.Create("Label Color")
                        .WithLabel("Label Color (Hex)")
                        .WithPlaceholder("#00FFFF")
                        .WithCharacterLimit(7);

                    yield return SliderTemplate.Create("Wear Level")
                        .WithLabel("Wear Level")
                        .WithDefaultValue(0)
                        .WithMinValue(0)
                        .WithMaxValue(255)
                        .WithIsInteger();

                    break;
                case 2:
                    yield return TextInputTemplate.Create("Keycard Label")
                        .WithLabel("Keycard Label")
                        .WithPlaceholder("Meth Card")
                        .WithCharacterLimit(ushort.MaxValue);

                    yield return TextInputTemplate.Create("Label Color")
                        .WithLabel("Label Color (Hex)")
                        .WithPlaceholder("#00FFFF")
                        .WithCharacterLimit(7);

                    break;
                case 3:
                    yield return TextInputTemplate.Create("Holder Name Input")
                        .WithLabel("Holder Name")
                        .WithDefaultText("A very cool person")
                        .WithCharacterLimit(ushort.MaxValue);

                    yield return TextInputTemplate.Create("Keycard Label")
                        .WithLabel("Keycard Label")
                        .WithPlaceholder("Meth Card")
                        .WithCharacterLimit(ushort.MaxValue);

                    yield return TextInputTemplate.Create("Label Color")
                        .WithLabel("Label Color (Hex)")
                        .WithPlaceholder("#00FFFF")
                        .WithCharacterLimit(7);

                    yield return SliderTemplate.Create("Wear Level")
                        .WithLabel("Wear Level")
                        .WithDefaultValue(0)
                        .WithMinValue(0)
                        .WithMaxValue(255)
                        .WithIsInteger();

                    yield return TextInputTemplate.Create("Serial Label")
                        .WithLabel("Serial Label")
                        .WithDefaultText("69420")
                        .WithCharacterLimit(ushort.MaxValue);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }

            yield return ButtonTemplate.Create("Button")
                .WithHandler(HandleButton)
                .WithButtonText("Create Keycard")
                .WithHoldTime(0.5F);
        }

        private static void HandleTypeDropdown(Player player, DropdownInstance dropdown)
        {
            if (!SelectedIndices.TryGetValue(player, out int index) || index != dropdown.IndexSelected)
                Menu.Update(player);

            SelectedIndices[player] = dropdown.IndexSelected;
        }

        private static void HandleButton(Player player, ButtonInstance button)
        {
            if (button.IsFirstResponse)
                return;

            int index = 0;
            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Type Selector"], out DropdownInstance? typeDropdown))
                index = typeDropdown.IndexSelected;

            string itemName = "Not defined";
            string holderName = "Not defined";
            string label = "Not defined";

            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Item Name"], out TextInputInstance? nameInput))
                itemName = nameInput.InputtedText;
            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Holder Name Input"], out TextInputInstance? holderInput))
                holderName = holderInput.InputtedText;
            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Keycard Label"], out TextInputInstance? labelInput))
                label = labelInput.InputtedText;

            int containment = 0;
            int armory = 0;
            int admin = 0;

            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Containment Level"], out SliderInstance? containmentSlider))
                containment = (int)containmentSlider.Value;
            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Armory Level"], out SliderInstance? armorySlider))
                armory = (int)armorySlider.Value;
            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Admin Level"], out SliderInstance? adminSlider))
                admin = (int)adminSlider.Value;

            KeycardLevels levels = new(containment, armory, admin);

            string keycardHex = "#000000";
            string permsHex = "#000000";
            string labelHex = "#000000";

            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Keycard Color"], out TextInputInstance? keycardHexInput))
                keycardHex = keycardHexInput.InputtedText;
            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Keycard Permissions Color"], out TextInputInstance? permsHexInput))
                permsHex = permsHexInput.InputtedText;
            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Label Color"], out TextInputInstance? labelHexInput))
                labelHex = labelHexInput.InputtedText;

            byte wearLevel = 0;

            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Wear Level"], out SliderInstance? wearLevelSlider))
                wearLevel = (byte)wearLevelSlider.Value;

            string serialLabel = string.Empty;

            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Serial Label"], out TextInputInstance? serialLabelInput))
                serialLabel = serialLabelInput.InputtedText;

            int rankIndex = 0;

            if (ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Rank Index"], out SliderInstance? rankIndexSlider))
                rankIndex = (int)rankIndexSlider.Value;

            TryParseHexColor(keycardHex, out Color keycardColor);
            TryParseHexColor(labelHex, out Color labelColor);
            TryParseHexColor(permsHex, out Color permsColor);

            Player target = ASSNetworking.TryGetSetting(player, [MenuRoot, MenuKey, "Player Selector"], out DropdownInstance? playerDropdown) ? Player.ReadyList.ElementAt(playerDropdown.IndexSelected) : player;

            switch (index)
            {
                case 0:
                    KeycardItem.CreateCustomKeycardTaskForce(target, itemName, holderName, levels, keycardColor, permsColor, serialLabel, rankIndex);
                    break;
                case 1:
                    KeycardItem.CreateCustomKeycardSite02(target, itemName, holderName, label, levels, keycardColor, permsColor, labelColor, wearLevel);
                    break;
                case 2:
                    KeycardItem.CreateCustomKeycardManagement(target, itemName, label, levels, keycardColor, permsColor, labelColor);
                    break;
                case 3:
                    KeycardItem.CreateCustomKeycardMetal(target, itemName, holderName, label, levels, keycardColor, permsColor, labelColor, wearLevel, serialLabel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeDropdown), typeDropdown, null);
            }
        }

        private static bool TryParseHexColor(string hex, out Color color)
        {
            color = Color.black; // default fallback

            if (string.IsNullOrEmpty(hex) || hex.Length != 7 || hex[0] != '#')
                return false;

            try
            {
                byte r = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);

                color = new Color(r / 255F, g / 255F, b / 255F);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}