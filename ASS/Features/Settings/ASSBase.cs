namespace ASS.Features.Settings
{
    using System;
    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif
    using LabApi.Features.Wrappers;
    using Mirror;
    using UserSettings.ServerSpecific;

    public abstract class ASSBase
    {
        public int Id { get; set; }

        public string? Label { get; set; }

        public string? Hint { get; set; }

        public Action<Player, ASSBase>? OnChanged { get; set; }

        public bool IgnoreNextResponse { get; set; }

        /// <summary>
        /// Gets the <see cref="ServerSpecificSettingBase.UserResponseMode"/> of the setting. Acquisition implies a response when the setting is received and Change implies a response whenever the setting is triggered.
        /// </summary>
        public abstract ServerSpecificSettingBase.UserResponseMode ResponseMode { get; }

        internal abstract Type SSSType { get; }

        #if EXILED
        internal HeaderSetting? ExHeader { get; init; }

        internal Action<Exiled.API.Features.Player, SettingBase>? ExAction { get; init; }

        public static implicit operator ASSBase(SettingBase exSetting)
        {
            ASSBase setting = exSetting switch
            {
                ButtonSetting button => (ASSButton)button,
                DropdownSetting dropdown => (ASSDropdown)dropdown,
                HeaderSetting header => (ASSHeader)header,
                KeybindSetting keybind => (ASSKeybind)keybind,
                SliderSetting slider => (ASSSlider)slider,
                TextInputSetting textDisplay => (ASSTextDisplay)textDisplay,
                UserTextInputSetting textInput => (ASSTextInput)textInput,
                TwoButtonsSetting twoButtons => (ASSTwoButtons)twoButtons,
                _ => throw new ArgumentOutOfRangeException(nameof(setting)),
            };

            return setting;
        }

        public static implicit operator SettingBase(ASSBase setting)
        {
            SettingBase exSetting = setting switch
            {
                ASSButton button => (ButtonSetting)button,
                ASSDropdown dropdown => (DropdownSetting)dropdown,
                ASSHeader header => (HeaderSetting)header,
                ASSKeybind keybind => (KeybindSetting)keybind,
                ASSSlider slider => (SliderSetting)slider,
                ASSTextDisplay textDisplay => (TextInputSetting)textDisplay,
                ASSTextInput textInput => (UserTextInputSetting)textInput,
                ASSTwoButtons twoButtons => (TwoButtonsSetting)twoButtons,
                _ => throw new ArgumentOutOfRangeException(nameof(setting)),
            };

            return exSetting;
        }
        #endif

        public static implicit operator ASSBase(ServerSpecificSettingBase bgSetting)
        {
            ASSBase setting = bgSetting switch
            {
                SSButton button => (ASSButton)button,
                SSDropdownSetting dropdown => (ASSDropdown)dropdown,
                SSGroupHeader header => (ASSHeader)header,
                SSKeybindSetting keybind => (ASSKeybind)keybind,
                SSSliderSetting slider => (ASSSlider)slider,
                SSTextArea textDisplay => (ASSTextDisplay)textDisplay,
                SSPlaintextSetting textInput => (ASSTextInput)textInput,
                SSTwoButtonsSetting twoButtons => (ASSTwoButtons)twoButtons,
                _ => throw new ArgumentOutOfRangeException(nameof(setting)),
            };

            return setting;
        }

        public static implicit operator ServerSpecificSettingBase(ASSBase setting)
        {
            ServerSpecificSettingBase bgSetting = setting switch
            {
                ASSButton button => (SSButton)button,
                ASSDropdown dropdown => (SSDropdownSetting)dropdown,
                ASSHeader header => (SSGroupHeader)header,
                ASSKeybind keybind => (SSKeybindSetting)keybind,
                ASSSlider slider => (SSSliderSetting)slider,
                ASSTextDisplay textDisplay => (SSTextArea)textDisplay,
                ASSTextInput textInput => (SSPlaintextSetting)textInput,
                ASSTwoButtons twoButtons => (SSTwoButtonsSetting)twoButtons,
                _ => throw new ArgumentOutOfRangeException(nameof(setting)),
            };

            return bgSetting;
        }

        public override string ToString()
        {
            return $"({Id}) {{{Label ?? "NULL"}}} [{Hint ?? "NULL"}] <{SSSType.Name}>";
        }

        internal virtual void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(Id);
            writer.WriteString(Label);
            writer.WriteString(Hint);
        }

        internal virtual void Deserialize(NetworkReaderPooled reader)
        {
            reader.Dispose();
        }

        internal abstract ASSBase Copy();
    }
}