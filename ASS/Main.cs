namespace ASS
{
    using System;
    using ASS.EventHandlers;
    using ASS.Features;
    using HarmonyLib;
    using LabApi.Events.Handlers;
    using LabApi.Features;
    using LabApi.Loader.Features.Plugins;
    using Mirror;
    using UserSettings.ServerSpecific;

    public class Main : Plugin<Config>
    {
        private static readonly Action HandlerAction = () => NetworkServer.ReplaceHandler<SSSClientResponse>(ASSNetworking.ProcessResponseMessage);

        private static Harmony harmony = null!;

        public static Main Instance { get; private set; } = null!;

        public override string Name => "ASS";

        public override string Author => "@Someone";

        public override Version Version { get; } = new(1, 0, 0);

        public override string Description => "Reworks SSS with the goal of improving player specific settings";

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        public override void Enable()
        {
            Instance = this;

            harmony = new Harmony("ASS");
            harmony.PatchAll();

            ServerSpecificSettingsSync.ServerOnStatusReceived += ASSNetworking.OnStatusReceived;

            CustomNetworkManager.OnClientReady += HandlerAction;

            PlayerEvents.Joined += Joined.OnJoined;
        }

        public override void Disable()
        {
            harmony.UnpatchAll(harmony.Id);

            ServerSpecificSettingsSync.ServerOnStatusReceived -= ASSNetworking.OnStatusReceived;

            CustomNetworkManager.OnClientReady -= HandlerAction;

            PlayerEvents.Joined -= Joined.OnJoined;
        }
    }
}