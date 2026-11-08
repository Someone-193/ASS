namespace ASS.Collections
{
    using System;
    using System.Collections.Generic;

    using ASS.Collections.EventHandlers;
    using ASS.Collections.Features;
    using ASS.Collections.Features.Pages;
    using ASS.Events.Handlers;
    using ASS.Features;
    using ASS.Features.Interfaces;

    using LabApi.Events.Handlers;
    using LabApi.Features;
    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;
    using LabApi.Loader.Features.Plugins;
    using LabApi.Loader.Features.Plugins.Enums;

    public class Main : Plugin<Config>
    {
        private static DateTime lastDependencyCheckMessage = DateTime.MinValue;
        private static bool enabled;

        // for other developers to use if necessary.
        public static bool RegisterPagesGenerator { get; set; } = true;

        public static Func<Player, IEnumerable<IASSObject>> PagesRootGenerator { get; } = viewer =>
        {
            Logger.Debug($"Finding settings for {viewer.Nickname}", Debug);

            if (Confirmation.Confirmations.TryGetValue(viewer, out Confirmation? confirmation))
            {
                Logger.Debug($"{viewer.Nickname} has a confirmation.", Debug);
                return confirmation.Templates();
            }

            if (!ASSPage.SelectedPage.TryGetValue(viewer, out Stack<ASSPage>? stack))
            {
                Logger.Debug($"{viewer.Nickname} has no selected page.", Debug);
                return ASSPage.MainPageController.GetChildren(viewer);
            }

            if (stack.TryPeek(out ASSPage? page))
            {
                Logger.Debug($"{viewer.Nickname} has a selected sub-page", Debug);
                return page.GetChildren(viewer);
            }
            else
            {
                Logger.Debug($"{viewer.Nickname} has no selected sub-page", Debug);
                Logger.Debug($"Primary page has {ASSPage.MainPageController.Children.Count} children", Debug);
                return ASSPage.MainPageController.GetChildren(viewer);
            }
        };

        public static Func<Player, bool> PagesBaseSettingsGenerator { get; } = viewer => ASSPage.SelectedPage.TryGetValue(viewer, out Stack<ASSPage>? stack) && stack.Count is 0;

        public static bool Debug
        {
            get
            {
                if (Instance is not null)
                    return Instance.Config?.Debug ?? false;

                // minimize log spam, but still have it keep warning, Instance can only be null if ASS was referenced and not enabled
                if ((DateTime.UtcNow - lastDependencyCheckMessage) > TimeSpan.FromSeconds(5))
                {
                    Logger.Warn("ASS.Collections is being used as a dependency (or is disabled yet still referenced by another plugin), please install it in the Plugins folder, or call EnsureInit from a consumer!");
                    lastDependencyCheckMessage = DateTime.UtcNow;
                }

                return false;
            }
        }

        public static Main? Instance { get; private set; }

        public override string Name => "ASS.Collections";

        public override string Author => "@Someone";

        public override Version Version { get; } = new(4, 0, 0);

        public override LoadPriority Priority => LoadPriority.High;

        public override string Description => "Adds pages to ASS";

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        public static void EnsureInit()
        {
            if (enabled)
                return;

            if (Instance is null)
            {
                Instance = new Main();
                Instance.LoadConfigs();
            }

            if (RegisterPagesGenerator)
            {
                ASSNetworking.TemplateRootGenerator = PagesRootGenerator;
                ASSNetworking.SendBaseGameSettingsGenerator = PagesBaseSettingsGenerator;
            }

            ASSPage.MainPageController.DisplayInHierarchy = false;
            ASSPage.MainPage.DisplayInHierarchy = false;

            PlayerEvents.Joined += Joined.OnJoined;
            PlayerEvents.Left += Left.OnLeft;

            SettingEvents.ObjectRegistered += ASSPage.OnObjectRegistered;
            SettingEvents.ObjectUnregistered += ASSPage.OnObjectUnregistered;

            enabled = true;
        }

        public static void DisablePlugin()
        {
            PlayerEvents.Joined -= Joined.OnJoined;
            PlayerEvents.Left -= Left.OnLeft;

            SettingEvents.ObjectRegistered -= ASSPage.OnObjectRegistered;
            SettingEvents.ObjectUnregistered -= ASSPage.OnObjectUnregistered;

            enabled = false;
        }

        public override void Enable()
        {
            Instance = this;

            EnsureInit();
        }

        public override void Disable() => DisablePlugin();
    }
}