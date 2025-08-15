namespace ASS.Example.PlayerMenuExamples
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using ASS.Events.EventArgs;
    using ASS.Features;
    using ASS.Features.Collections;
    using ASS.Features.Settings;

    using Interactables.Interobjects.DoorUtils;

    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Features.Wrappers;

    using UnityEngine;

    // Idea yoinked from adyman0010 while trying to debug ASS :P
    public class KeycardCreatorExample
    {
        private static readonly Dictionary<Player, PlayerMenu> Menus = new();

        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            foreach (KeyValuePair<Player, PlayerMenu> kvp in Menus)
                kvp.Value.Update(false, true);

            Menus[ev.Player] = new PlayerMenu(Generator, ev.Player);
        }

        public static void OnLeft(PlayerLeftEventArgs ev)
        {
            foreach (KeyValuePair<Player, PlayerMenu> kvp in Menus)
                kvp.Value.Update(false, true);

            if (Menus.TryGetValue(ev.Player, out PlayerMenu menu))
                menu.Destroy();
        }

        public static void OnSettingTriggered(SettingTriggeredEventArgs ev)
        {
            switch (ev.Setting)
            {
                case ASSDropdown { Id: 102 }:
                    if (Menus.TryGetValue(ev.Player, out PlayerMenu menu))
                        menu.Update(false, true);

                    break;
                case ASSButton { Id: 115 }:
                    int index = 0;
                    if (ASSNetworking.TryGetSetting(ev.Player, 102, out ASSDropdown typeDropdown))
                        index = typeDropdown.IndexSelected;

                    string itemName = "Not defined";
                    string holderName = "Not defined";
                    string label = "Not defined";

                    if (ASSNetworking.TryGetSetting(ev.Player, 103, out ASSTextInput nameInput))
                        itemName = nameInput.InputtedText;
                    if (ASSNetworking.TryGetSetting(ev.Player, 109, out ASSTextInput holderInput))
                        holderName = holderInput.InputtedText;
                    if (ASSNetworking.TryGetSetting(ev.Player, 110, out ASSTextInput labelInput))
                        label = labelInput.InputtedText;

                    int containment = 0;
                    int armory = 0;
                    int admin = 0;

                    if (ASSNetworking.TryGetSetting(ev.Player, 104, out ASSSlider containmentSlider))
                        containment = (int)containmentSlider.Value;
                    if (ASSNetworking.TryGetSetting(ev.Player, 105, out ASSSlider armorySlider))
                        armory = (int)armorySlider.Value;
                    if (ASSNetworking.TryGetSetting(ev.Player, 106, out ASSSlider adminSlider))
                        admin = (int)adminSlider.Value;

                    KeycardLevels levels = new(containment, armory, admin);

                    string keycardHex = "#000000";
                    string permsHex = "#000000";
                    string labelHex = "#000000";

                    if (ASSNetworking.TryGetSetting(ev.Player, 107, out ASSTextInput keycardHexInput))
                        keycardHex = keycardHexInput.InputtedText;
                    if (ASSNetworking.TryGetSetting(ev.Player, 108, out ASSTextInput permsHexInput))
                        permsHex = permsHexInput.InputtedText;
                    if (ASSNetworking.TryGetSetting(ev.Player, 111, out ASSTextInput labelHexInput))
                        labelHex = labelHexInput.InputtedText;

                    byte wearLevel = 0;

                    if (ASSNetworking.TryGetSetting(ev.Player, 112, out ASSSlider wearLevelSlider))
                        wearLevel = (byte)wearLevelSlider.Value;

                    string serialLabel = string.Empty;

                    if (ASSNetworking.TryGetSetting(ev.Player, 113, out ASSTextInput serialLabelInput))
                        serialLabel = serialLabelInput.InputtedText;

                    int rankIndex = 0;

                    if (ASSNetworking.TryGetSetting(ev.Player, 114, out ASSSlider rankIndexSlider))
                        rankIndex = (int)rankIndexSlider.Value;

                    TryParseHexColor(keycardHex, out Color keycardColor);
                    TryParseHexColor(labelHex, out Color labelColor);
                    TryParseHexColor(permsHex, out Color permsColor);

                    switch (index)
                    {
                        case 0:
                            KeycardItem.CreateCustomKeycardTaskForce(ev.Player, itemName, holderName, levels, keycardColor, permsColor, serialLabel, rankIndex);
                            break;
                        case 1:
                            KeycardItem.CreateCustomKeycardSite02(ev.Player, itemName, holderName, label, levels, keycardColor, permsColor, labelColor, wearLevel);
                            break;
                        case 2:
                            KeycardItem.CreateCustomKeycardManagement(ev.Player, itemName, label, levels, keycardColor, permsColor, labelColor);
                            break;
                        case 3:
                            KeycardItem.CreateCustomKeycardMetal(ev.Player, itemName, holderName, label, levels, keycardColor, permsColor, labelColor, wearLevel, serialLabel);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(ev.Setting), ev.Setting, null);
                    }

                    break;
            }
        }

        private static ASSGroup Generator(Player owner)
        {
            int index = 0;
            if (ASSNetworking.TryGetSetting(owner, 102, out ASSDropdown dropdown))
                index = dropdown.IndexSelected;

            List<ASSBase> settings = 
                [
                    new ASSHeader(100, "Keycard Creator"),
                    new ASSDropdown(101, "Player:", Player.ReadyList.Select(p => p.Nickname).ToArray(), hint: "The player who will receive the keycard"),
                    new ASSDropdown(102, "Keycard Type", [ItemType.KeycardCustomTaskForce.ToString(), ItemType.KeycardCustomSite02.ToString(), ItemType.KeycardCustomManagement.ToString(), ItemType.KeycardCustomMetalCase.ToString()]),
                    new ASSTextInput(103, "Item Name", "My Cool Keycard!"),
                    new ASSSlider(104, "Containment Level", 1, 0, 3, true),
                    new ASSSlider(105, "Armory Level", 2, 0, 3, true),
                    new ASSSlider(106, "Admin Level", 3, 0, 3, true),
                    new ASSTextInput(107, "Keycard Color (Hex)", "#00FFFF", 7),
                    new ASSTextInput(108, "Keycard Permissions Color (Hex)", "#FF0000", 7),
                ];

            switch (index)
            {
                case 0:
                    settings.AddRange([
                        new ASSTextInput(109, "Holder Name", "A very cool person"),
                        new ASSTextInput(113, "Serial Label", "69420"),
                        new ASSSlider(114, "Rank Index", 0, 0, 100, true),
                    ]);
                    break;
                case 1:
                    settings.AddRange([
                        new ASSTextInput(109, "Holder Name", "A very cool person"),
                        new ASSTextInput(110, "Keycard Label", "Meth Card"),
                        new ASSTextInput(111, "Label Color (Hex)", "#00FFFF", 7),
                        new ASSSlider(112, "Wear Level", 0, 0, 255, true),
                    ]);
                    break;
                case 2:
                    settings.AddRange([
                        new ASSTextInput(110, "Keycard Label", "Meth Card"),
                        new ASSTextInput(111, "Label Color (Hex)", "#00FFFF", 7),
                    ]);
                    break;
                case 3:
                    settings.AddRange([
                        new ASSTextInput(109, "Holder Name", "A very cool person"),
                        new ASSTextInput(110, "Keycard Label", "Meth Card"),
                        new ASSTextInput(111, "Label Color (Hex)", "#00FFFF", 7),
                        new ASSSlider(112, "Wear Level", 0, 0, 255, true),
                        new ASSTextInput(113, "Serial Label", "69420"),
                    ]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }

            settings.Add(new ASSButton(115, "Create Keycard", holdTime: 0.5F));

            return new ASSGroup(settings, 0, p => p == owner);
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