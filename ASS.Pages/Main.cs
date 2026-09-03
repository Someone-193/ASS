namespace ASS.Pages
{
    using System;

    using LabApi.Features;
    using LabApi.Loader.Features.Plugins;

    #if EXILED
    public class Main : Exiled.API.Features.Plugin<Config>
    #elif LABAPI
    public class Main : Plugin
    #endif
    {
        public override string Name => "ASS.Pages";

        public override string Author => "@Someone";

        public override Version Version { get; } = new(1, 0, 0);

        #if EXILED
        public override string Prefix => "ASS_Pages";
        #elif LABAPI
        public override string Description => "Adds pages to ASS";

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
        #endif

        #if EXILED
        public override void OnEnabled()
        {
            Enabled();
        }

        public override void OnDisabled()
        {
            Disabled();
        }
        #elif LABAPI
        public override void Enable()
        {
            Enabled();
        }

        public override void Disable()
        {
            Disabled();
        }
        #endif

        private void Enabled()
        {
        }

        private void Disabled()
        {
        }
    }
}