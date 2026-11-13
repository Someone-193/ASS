namespace ASS.ASSML
{
    using System;
    using LabApi.Features;
    using LabApi.Features.Console;
    using LabApi.Loader.Features.Plugins;
    using LabApi.Loader.Features.Plugins.Enums;

    // insert guy writing fire gif
    public class Main : Plugin<Config>
    {
        private static DateTime lastDependencyCheckMessage = DateTime.MinValue;
        private static bool enabled;

        public static bool Debug
        {
            get
            {
                if (Instance is not null)
                    return Instance.Config?.Debug ?? false;

                // minimize log spam, but still have it keep warning, Instance can only be null if ASS was referenced and not enabled
                if ((DateTime.UtcNow - lastDependencyCheckMessage) > TimeSpan.FromSeconds(5))
                {
                    Logger.Warn("ASS.ASSML is being used as a dependency (or is disabled yet still referenced by another plugin), please install it in the Plugins folder, or call EnsureInit from a consumer!");
                    lastDependencyCheckMessage = DateTime.UtcNow;
                }

                return false;
            }
        }

        public static Main? Instance { get; private set; }

        public override string Name => "ASS.ASSML";

        public override string Author => "@Someone";

        public override Version Version { get; } = new(4, 0, 0);

        public override string Description => "Allows creation and loading of ASS collections via .assml files.";

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        public override LoadPriority Priority => LoadPriority.High - 1;

        public static void EnsureInit()
        {
            if (enabled)
                return;

            if (Instance is null)
            {
                Instance = new Main();
                Instance.LoadConfigs();
            }

            enabled = true;
        }

        public static void DisablePlugin()
        {
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