namespace ASS.Example.PlayerMenuExamples
{
    using ASS.Features.Collections;
    using ASS.Features.Settings;

    #if EXILED
    using Player = Exiled.API.Features.Player;
    #endif

    using LabApi.Features.Wrappers;

    public class AbstractExample : AbstractMenu
    {
        public static AbstractExample Instance { get; } = new();

        protected override ASSGroup Generate(Player owner)
        {
            return new ASSGroup([new ASSHeader(-14, "This came from an AbstractMenu")], viewers: p => p == owner);
        }
    }
}