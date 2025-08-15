namespace ASS.Features.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ASS.Features.MirrorUtils.Messages;
    using ASS.Features.Settings.Displays;

    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif

    using LabApi.Features.Wrappers;
    using Mirror;
    using UserSettings.ServerSpecific;

    public abstract class ASSBase
    {
        private string? label;
        private string? hint;

        public int Id { get; set; }

        public string? Label
        {
            get => label;
            set
            {
                label = value;
                if (AutoSync && IsInstance)
                    Update(this.SettingHolders());
            }
        }

        public string? Hint
        {
            get => hint;
            set
            {
                hint = value;
                if (AutoSync && IsInstance)
                    Update(this.SettingHolders());
            }
        }

        public byte CollectionId { get; set; } = byte.MaxValue;

        public Action<Player, ASSBase>? OnChanged { get; set; }

        public bool IgnoreNextResponse { get; set; }

        public virtual bool IsServerOnly => false;

        /// <summary>
        /// Gets or sets a value indicating whether the properties of this setting automatically sync to clients
        /// <br/>(if true, changing properties like <see cref="Label"/> will automatically update on clients).
        /// </summary>
        public bool AutoSync { get; set; } = true;

        /// <summary>
        /// Gets the <see cref="ServerSpecificSettingBase.UserResponseMode"/> of the setting. Acquisition implies a response when the setting is received and Change implies a response whenever the setting is triggered.
        /// </summary>
        public abstract ServerSpecificSettingBase.UserResponseMode ResponseMode { get; }

        internal abstract Type SSSType { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this setting is an original setting or a copy (Only copies can sync values, so this reduces required and accidental enumeration in setters).
        /// </summary>
        internal bool IsInstance { get; set; }

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

        /// <summary>
        /// Updates the <see cref="Label"/> of a specified setting for clients.
        /// </summary>
        /// <param name="newLabel">The new label.</param>
        /// <param name="players">Those who receive the update.</param>
        public void UpdateLabel(string? newLabel, IEnumerable<Player>? players)
        {
            MirrorUtils.ASSUtils.SendASSMessageToPlayersConditionally(
                new ASSUpdateMessage(this, writer =>
                {
                    writer.WriteBool(true);
                    writer.WriteString(newLabel);
                    writer.WriteString(Hint);
                }), players is null ? p => this.SettingHolders().Contains(p) : players.Contains);
        }

        /// <summary>
        /// Updates the <see cref="Hint"/> of a specified setting for clients.
        /// </summary>
        /// <param name="newHint">The new hint.</param>
        /// <param name="players">Those who receive the update.</param>
        public void UpdateHint(string? newHint, IEnumerable<Player>? players)
        {
            MirrorUtils.ASSUtils.SendASSMessageToPlayersConditionally(
                new ASSUpdateMessage(this, writer =>
                {
                    writer.WriteBool(true);
                    writer.WriteString(Label);
                    writer.WriteString(newHint);
                }), players is null ? p => this.SettingHolders().Contains(p) : players.Contains);
        }

        /// <summary>
        /// Sync the specified settings <see cref="Label"/> and <see cref="Hint"/> for clients.
        /// </summary>
        /// <param name="players">Those who receive the update.</param>
        public void Update(IEnumerable<Player>? players)
        {
            MirrorUtils.ASSUtils.SendASSMessageToPlayersConditionally(
                new ASSUpdateMessage(this, writer =>
                {
                    writer.WriteBool(true);
                    writer.WriteString(Label);
                    writer.WriteString(Hint);
                }), players is null ? p => this.SettingHolders().Contains(p) : players.Contains);
        }

        public override string ToString()
        {
            return $"({Id}) {{{Label ?? "NULL"}}} [{Hint ?? "NULL"}] <{SSSType.Name}>";
        }

        // for derived update methods
        internal void UpdateDerived(Action<NetworkWriter> action, IEnumerable<Player>? players)
        {
            MirrorUtils.ASSUtils.SendASSMessageToPlayersConditionally(
                new ASSUpdateMessage(this, writer =>
                {
                    writer.WriteBool(false);
                    action(writer);
                }), players is null ? p => this.SettingHolders().Contains(p) : players.Contains);
        }

        internal virtual void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(Id);
            writer.WriteString(Label);
            writer.WriteString(Hint);
            writer.WriteByte(CollectionId);
            writer.WriteBool(IsServerOnly);
        }

        internal virtual void Deserialize(NetworkReaderPooled reader)
        {
            reader.Dispose();
        }

        internal abstract ASSBase Copy();
    }
}