namespace ASS.Pages
{
    using System;
    using System.Collections.Generic;

    using ASS.Events.Handlers;
    using ASS.Features;
    using ASS.Features.Interfaces;
    using ASS.Pages.EventHandlers;
    using ASS.Pages.Features;

    using LabApi.Events.Handlers;
    using LabApi.Features;
    using LabApi.Features.Wrappers;
    using LabApi.Loader.Features.Plugins;
    using LabApi.Loader.Features.Plugins.Enums;

    public class Main : Plugin
    {
        // for other developers to use if necessary.
        public static bool RegisterPagesGenerator { get; set; } = true;

        public static Func<Player, IEnumerable<IASSObject>> PagesRootGenerator { get; } = viewer =>
        {
            if (!ASSPage.SelectedPage.TryGetValue(viewer, out Stack<ASSPage>? stack))
                return [];

            return stack.TryPeek(out ASSPage? page)
                ? page.GetChildren(viewer)
                : ASSPage.MainPageController.GetChildren(viewer);
        };

        public static Func<Player, bool> PagesBaseSettingsGenerator { get; } = viewer => ASSPage.SelectedPage.TryGetValue(viewer, out Stack<ASSPage>? stack) && stack.Count is 0;

        public override string Name => "ASS.Pages";

        public override string Author => "@Someone";

        public override Version Version { get; } = new(4, 0, 0);

        public override LoadPriority Priority => LoadPriority.High - 1;

        public override string Description => "Adds pages to ASS";

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        public override void Enable()
        {
            if (RegisterPagesGenerator)
            {
                ASSNetworking.TemplateRootGenerator = PagesRootGenerator;
                ASSNetworking.SendBaseGameSettingsGenerator = PagesBaseSettingsGenerator;
            }

            PlayerEvents.Joined += Joined.OnJoined;
            PlayerEvents.Left += Left.OnLeft;

            SettingEvents.ObjectRegistered += ASSPage.OnObjectRegistered;
            SettingEvents.ObjectUnregistered += ASSPage.OnObjectUnregistered;
        }

        public override void Disable()
        {
            PlayerEvents.Joined -= Joined.OnJoined;
            PlayerEvents.Left -= Left.OnLeft;

            SettingEvents.ObjectRegistered -= ASSPage.OnObjectRegistered;
            SettingEvents.ObjectUnregistered -= ASSPage.OnObjectUnregistered;
        }
    }
}