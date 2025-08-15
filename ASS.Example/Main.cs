namespace ASS.Example
{
    using System;

    using ASS.Events.Handlers;
    using ASS.Example.DefaultExample;
    using ASS.Example.EventExample;
    using ASS.Example.PlayerMenuExamples;
    using ASS.Features;

    using LabApi.Events.Handlers;

    using LabApi.Features;
    using LabApi.Loader.Features.Plugins;

    #if EXILED
    public class Main : Exiled.API.Features.Plugin<Config>
    #elif LABAPI
    public class Main : Plugin
    #endif
    {
        public override string Name => "ASS.Example";

        public override string Author => "@Someone";

        public override Version Version { get; } = new(2, 0, 0);

        #if EXILED
        public override string Prefix => "ASS_Example";
        #elif LABAPI
        public override string Description => "A showcase of ASS";

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
        #endif

        #if EXILED
        public override void OnEnabled()
        {
            SettingEvents.SettingTriggered += SettingTriggered.OnSettingTriggered;
            SettingEvents.ButtonPressed += ButtonPressed.OnButtonPressed;

            PlayerEvents.Joined += WelcomeSetting.OnJoined;
            PlayerEvents.Left += WelcomeSetting.OnLeft;

            PlayerEvents.GroupChanged += AdminMenu.OnChangedGroup;
            PlayerEvents.Left += AdminMenu.OnLeft;
            SettingEvents.SettingTriggered += AdminMenu.OnSettingTriggered;
        }

        public override void OnDisabled()
        {
            SettingEvents.SettingTriggered -= SettingTriggered.OnSettingTriggered;
            SettingEvents.ButtonPressed -= ButtonPressed.OnButtonPressed;

            PlayerEvents.Joined -= WelcomeSetting.OnJoined;
            PlayerEvents.Left -= WelcomeSetting.OnLeft;

            PlayerEvents.GroupChanged -= AdminMenu.OnChangedGroup;
            PlayerEvents.Left -= AdminMenu.OnLeft;
            SettingEvents.SettingTriggered -= AdminMenu.OnSettingTriggered;
        }
        #elif LABAPI
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
        #endif
    }
}