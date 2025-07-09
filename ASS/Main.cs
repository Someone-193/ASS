namespace ASS
{
    using System;
    using ASS.Settings;
    using HarmonyLib;
    using LabApi.Features;
    using LabApi.Loader.Features.Plugins;
    using Mirror;
    using UserSettings.ServerSpecific;

    public class Main : Plugin<Config>
    {
        private static Harmony harmony = null!;

        public override string Name => "ASS";

        public override string Author => "@Someone";

        public override Version Version { get; } = new(1, 0, 0);

        public override string Description => "Reworks SSS with the goal of improving player specific settings";

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        public override void Enable()
        {
            harmony = new Harmony("ASS");
            harmony.PatchAll();

            NetworkServer.ReplaceHandler<SSSClientResponse>(ASSNetworking.ProcessResponseMessage);
        }

        public override void Disable()
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }
}