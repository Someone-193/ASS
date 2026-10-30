namespace ASS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ASS.EventHandlers;
    using ASS.Features;
    using ASS.Features.Interfaces;

    using HarmonyLib;

    using LabApi.Events.Handlers;
    using LabApi.Features;
    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;
    using LabApi.Loader.Features.Plugins;
    using LabApi.Loader.Features.Plugins.Enums;

    using Mirror;

    using UserSettings.ServerSpecific;

    public class Main : Plugin<Config>
    {
        private static readonly Action HandlerAction = () => NetworkServer.ReplaceHandler<SSSClientResponse>(ASSNetworking.ProcessResponseMessage);
        private static DateTime lastDependencyCheckMessage = DateTime.MinValue;

        private static Harmony harmony = null!;

        public static bool Debug
        {
            get
            {
                if (Instance is not null)
                    return Instance.Config?.Debug ?? false;

                // minimize log spam, but still have it keep warning, Instance can only be null if ASS was referenced and not enabled
                if ((DateTime.UtcNow - lastDependencyCheckMessage) > TimeSpan.FromSeconds(5))
                {
                    Logger.Warn("ASS is being used as a dependency (or is disabled yet still referenced by another plugin), please install it in the Plugins folder!");
                    lastDependencyCheckMessage = DateTime.UtcNow;
                }

                return false;
            }
        }

        public static Main? Instance { get; private set; }

        // the default template generator.
        public static Func<Player, IEnumerable<IASSObject>> StandardTemplateRootGenerator { get; } = _ => ASSNetworking.Objects.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);

        public static Func<Player, bool> StandardSendBaseGameSettingsGenerator { get; } = _ => true;

        public override string Name => "ASS";

        public override string Author => "@Someone";

        public override Version Version { get; } = new(2, 0, 0);

        public override LoadPriority Priority => LoadPriority.High;

        public override string Description => "Reworks SSS with the goal of improving player specific settings";

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        public override void Enable()
        {
            Instance = this;

            ServerSpecificSettingsSync.SendOnJoinFilter = _ => false;

            harmony = new Harmony("ASS");
            harmony.PatchAll();

            ServerSpecificSettingsSync.ServerOnStatusReceived += ASSNetworking.OnStatusReceived;

            CustomNetworkManager.OnClientReady += HandlerAction;

            PlayerEvents.Joined += Joined.OnJoined;
            PlayerEvents.Left += Left.OnLeft;

            ServerSpecificSettingsSync.DefinedSettings ??= [];
        }

        public override void Disable()
        {
            harmony.UnpatchAll(harmony.Id);

            ServerSpecificSettingsSync.ServerOnStatusReceived -= ASSNetworking.OnStatusReceived;

            CustomNetworkManager.OnClientReady -= HandlerAction;

            PlayerEvents.Joined -= Joined.OnJoined;
            PlayerEvents.Left -= Left.OnLeft;
        }
    }
}