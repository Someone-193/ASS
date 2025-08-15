namespace ASS.Events.Handlers
{
    using ASS.Events.EventArgs;

    using LabApi.Events;

    public static class SettingEvents
    {
        /// <summary>
        /// Called whenever a setting is sent to a client.
        /// </summary>
        /// <remarks>
        /// Can be called multiple times (for keybinds) by one SendToPlayer call because of ASS's minimization logic.
        /// <br/>
        /// Useful for ignoring responses from particular settings manually.
        /// </remarks>
        public static event LabEventHandler<SendingSettingEventArgs>? SendingSetting;

        public static event LabEventHandler<UpdatingSettingEventArgs>? UpdatingSetting;

        public static event LabEventHandler<SettingTriggeredEventArgs>? SettingTriggered = ev => ev.Setting.OnChanged?.Invoke(ev.Player, ev.Setting);

        public static event LabEventHandler<KeybindPressedEventArgs>? KeybindPressed;

        public static event LabEventHandler<DropdownTriggeredEventArgs>? DropdownTriggered;

        public static event LabEventHandler<TwoButtonsPressedEventArgs>? TwoButtonsPressed;

        public static event LabEventHandler<SliderMovedEventArgs>? SliderMoved;

        public static event LabEventHandler<ButtonPressedEventArgs>? ButtonPressed;

        public static event LabEventHandler<TextInputChangedEventArgs>? TextInputChanged;

        public static void OnSendingSetting(SendingSettingEventArgs ev) => SendingSetting?.InvokeEvent(ev);

        public static void OnUpdatingSetting(UpdatingSettingEventArgs ev) => UpdatingSetting?.InvokeEvent(ev);

        public static void OnSettingTriggered(SettingTriggeredEventArgs ev) => SettingTriggered?.InvokeEvent(ev);

        public static void OnKeybindPressed(KeybindPressedEventArgs ev) => KeybindPressed?.InvokeEvent(ev);

        public static void OnDropdownTriggered(DropdownTriggeredEventArgs ev) => DropdownTriggered?.InvokeEvent(ev);

        public static void OnTwoButtonsPressed(TwoButtonsPressedEventArgs ev) => TwoButtonsPressed?.Invoke(ev);

        public static void OnSliderMoved(SliderMovedEventArgs ev) => SliderMoved?.Invoke(ev);

        public static void OnButtonPressed(ButtonPressedEventArgs ev) => ButtonPressed?.Invoke(ev);

        public static void OnTextInputChanged(TextInputChangedEventArgs ev) => TextInputChanged?.Invoke(ev);
    }
}