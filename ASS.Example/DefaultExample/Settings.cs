namespace ASS.Example.DefaultExample
{
    using System.Collections.Generic;
    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;
    using LabApi.Features.Wrappers;

    public static class Settings
    {
        public static List<IASSObject> WelcomeHeader(Player viewer) => [HeaderTemplate.Create("Header").WithLabel($"Welcome {viewer.Nickname}!")];

        public static void HandleButtonOne(Player player, ButtonInstance button)
        {
            // technically redundant as buttons do not have an initial response.
            if (button.IsInitialResponse)
                return;

            player.Kill($"Hit the funny default button with key {button.Key}");
        }

        public static void HandleButtonTwo(Player player, ButtonInstance button)
        {
            // technically redundant as buttons do not have an initial response.
            if (button.IsInitialResponse)
                return;

            player.Kill("Hit the funny event button");
        }
    }
}