namespace ASS
{
    using System;
    using ASS.EventHandlers;
    using ASS.Features;

    #if EXILED
    using Exiled.API.Enums;
    #endif
    using HarmonyLib;
    using LabApi.Events.Handlers;
    using Mirror;
    using UserSettings.ServerSpecific;

    #if EXILED
    public class Main : Exiled.API.Features.Plugin<Config>
    #elif LABAPI
    public class Main : Plugin<Config>
    #endif
    {
        private static readonly Action HandlerAction = () => NetworkServer.ReplaceHandler<SSSClientResponse>(ASSNetworking.ProcessResponseMessage);

        private static Harmony harmony = null!;

        public static bool Debug => Instance.Config?.Debug ?? false;

        public static Main Instance { get; private set; } = null!;

        public override string Name => "ASS";

        public override string Author => "@Someone";

        public override Version Version { get; } = new(2, 0, 0);

        #if EXILED
        public override string Prefix => "ASS";

        public override PluginPriority Priority => PluginPriority.Higher;
        #elif LABAPI
        public override LoadPriority Priority => LoadPriority.High;

        public override string Description => "Reworks SSS with the goal of improving player specific settings";

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
        #endif

        #if EXILED
        public override void OnEnabled()
        {
            Instance = this;

            harmony = new Harmony("ASS");
            harmony.PatchAll();

            ServerSpecificSettingsSync.ServerOnStatusReceived += ASSNetworking.OnStatusReceived;

            CustomNetworkManager.OnClientReady += HandlerAction;

            PlayerEvents.Joined += Joined.OnJoined;
            PlayerEvents.Left += Left.OnLeft;

            ServerSpecificSettingsSync.SendOnJoinFilter = _ => false;
            ServerSpecificSettingsSync.DefinedSettings ??= [];
        }

        public override void OnDisabled()
        {
            harmony.UnpatchAll(harmony.Id);

            ServerSpecificSettingsSync.ServerOnStatusReceived -= ASSNetworking.OnStatusReceived;

            CustomNetworkManager.OnClientReady -= HandlerAction;

            PlayerEvents.Joined -= Joined.OnJoined;
            PlayerEvents.Left -= Left.OnLeft;
        }
        #elif LABAPI
        public override void Enable()
        {
            Instance = this;

            harmony = new Harmony("ASS");
            harmony.PatchAll();

            ServerSpecificSettingsSync.ServerOnStatusReceived += ASSNetworking.OnStatusReceived;

            CustomNetworkManager.OnClientReady += HandlerAction;

            PlayerEvents.Joined += Joined.OnJoined;

            ServerSpecificSettingsSync.DefinedSettings ??= [];
        }

        public override void Disable()
        {
            harmony.UnpatchAll(harmony.Id);

            ServerSpecificSettingsSync.ServerOnStatusReceived -= ASSNetworking.OnStatusReceived;

            CustomNetworkManager.OnClientReady -= HandlerAction;

            PlayerEvents.Joined -= Joined.OnJoined;
        }
        #endif
    }
}