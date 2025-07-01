namespace ASS.Example
{
    using System;
    using ASS.Example.DefaultExample;
    using ASS.Example.EventExample;
    using ASS.Example.PlayerMenuExamples;
    using ASS.Settings;
    using LabApi.Events.Handlers;
    using LabApi.Features;
    using LabApi.Loader.Features.Plugins;

    public class Main : Plugin
    {
        public override string Name => "ASS.Example";

        public override string Description => "A showcase of ASS";

        public override string Author => "@Someone";

        public override Version Version { get; } = new(1, 0, 0);

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        public override void Enable()
        {
            ASSNetworking.SettingTriggered += SettingTriggered.OnSettingTriggered;
            ASSNetworking.ButtonPressed += ButtonPressed.OnButtonPressed;

            PlayerEvents.Joined += WelcomeSetting.OnJoined;
            PlayerEvents.Left += WelcomeSetting.OnLeft;

            PlayerEvents.GroupChanged += AdminMenu.OnChangedGroup;
            PlayerEvents.Left += AdminMenu.OnLeft;
            ASSNetworking.SettingTriggered += AdminMenu.OnSettingTriggered;
        }

        public override void Disable()
        {
            ASSNetworking.SettingTriggered -= SettingTriggered.OnSettingTriggered;
            ASSNetworking.ButtonPressed -= ButtonPressed.OnButtonPressed;

            PlayerEvents.Joined -= WelcomeSetting.OnJoined;
            PlayerEvents.Left -= WelcomeSetting.OnLeft;

            PlayerEvents.GroupChanged -= AdminMenu.OnChangedGroup;
            PlayerEvents.Left -= AdminMenu.OnLeft;
            ASSNetworking.SettingTriggered -= AdminMenu.OnSettingTriggered;
        }
    }
}