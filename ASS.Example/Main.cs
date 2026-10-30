namespace ASS.Example
{
    using System;

    using ASS.Example.DefaultExample;
    using ASS.Example.PagesExamples;
    using ASS.Example.PlayerMenuExamples;
    using ASS.Features.Containers;
    using ASS.Features.Templates;
    using ASS.Pages.Features;

    using LabApi.Events.Handlers;
    using LabApi.Features;
    using LabApi.Loader.Features.Plugins;

    public class Main : Plugin
    {
        private static ASSGroup group = null!;

        public override string Name => "ASS.Example";

        public override string Author => "@Someone";

        public override Version Version { get; } = new(2, 0, 0);

        public override string Description => "A showcase of ASS";

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        public override void Enable()
        {
            PlayerEvents.Joined += KeycardCreatorExample.OnJoined;
            PlayerEvents.Left += KeycardCreatorExample.OnLeft;

            PlayerEvents.GroupChanged += AdminMenu.OnChangedGroup;
            PlayerEvents.Left += AdminMenu.OnLeft;

            group = ASSGroup
                .Create("ASS.Example")
                .WithChildren([
                    PlayerMenu.Create("Welcome Setting", Settings.WelcomeHeader),
                    ButtonTemplate
                        .Create("Button 1")
                        .WithHandler(Settings.HandleButtonOne)
                        .WithLabel("Test 1"),
                    ButtonTemplate
                        .Create("Button 2")
                        .WithHandler(Settings.HandleButtonTwo)
                        .WithLabel("Test 2"),
                    AdminMenu.Menu,
                    KeycardCreatorExample.Menu,
                    PageExample.Page,
                ])
                .Register(0);

            ASSPage.MainPageController.AddPage("Example Primary Page", ASSGroup
                    .Create("ASS.Example Primary Group Example")
                    .WithChildren([
                        HeaderTemplate
                            .Create("Header")
                            .WithLabel("Page Example"),
                        SliderTemplate
                            .Create("Slider")
                            .WithHandler((p, s) => p.Health = s.Value)
                            .WithMinValue(0)
                            .WithMaxValue(100)
                            .WithDefaultValue(100)
                            .WithLabel("Health Controller")
                    ]));
        }

        public override void Disable()
        {
            PlayerEvents.Joined -= KeycardCreatorExample.OnJoined;
            PlayerEvents.Left -= KeycardCreatorExample.OnLeft;

            PlayerEvents.GroupChanged -= AdminMenu.OnChangedGroup;
            PlayerEvents.Left -= AdminMenu.OnLeft;

            group.Unregister();
        }
    }
}