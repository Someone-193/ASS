namespace ASS.Events.Handlers
{
    using ASS.Events.EventArgs;

    public static class SettingEvents
    {
        /// <summary>
        /// Gets or sets an event called whenever a setting is sent to a client.
        /// </summary>
        /// <remarks>
        /// Can be called multiple times (for keybinds) by one SendToPlayer call because of ASS's minimization logic.
        /// <br/>
        /// Useful for ignoring responses from particular settings manually.
        /// </remarks>
        public static ASSEvent<SendingInstancesEventArgs> SendingSettings { get; set; } = new();

        public static ASSEvent<UpdatingInstanceEventArgs> UpdatingSetting { get; set; } = new();

        public static ASSEvent<InstanceTriggeredEventArgs> SettingTriggered { get; set; } = new();

        public static ASSEvent<ObjectRegisteredEventArgs> ObjectRegistered { get; set; } = new();

        public static ASSEvent<ObjectUnregisteredEventArgs> ObjectUnregistered { get; set; } = new();

        public static void OnSendingSettings(SendingInstancesEventArgs ev) => SendingSettings.Invoke(ev);

        public static void OnUpdatingSetting(UpdatingInstanceEventArgs ev) => UpdatingSetting.Invoke(ev);

        public static void OnSettingTriggered(InstanceTriggeredEventArgs ev) => SettingTriggered.Invoke(ev);

        public static void OnObjectRegistered(ObjectRegisteredEventArgs ev) => ObjectRegistered.Invoke(ev);

        public static void OnObjectUnregistered(ObjectUnregisteredEventArgs ev) => ObjectUnregistered.Invoke(ev);
    }
}