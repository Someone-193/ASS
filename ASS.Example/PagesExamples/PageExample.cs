namespace ASS.Example.PagesExamples
{
    using ASS.Collections.Features.Pages;
    using ASS.Features.Containers;
    using ASS.Features.Instances;
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    public class PageExample
    {
        public static ButtonPage Page { get; } =
            ButtonPage
                .Create("ASS.Example Button Page")
                .WithChildren([
                    ASSGroup
                        .Create("Button Page Child")
                        .WithChildren([
                            HeaderTemplate
                                .Create("Header")
                                .WithLabel("Button Page Header"),
                            ButtonTemplate
                                .Create("Kill Button")
                                .WithHandler(Handler)
                                .WithLabel("Kill Button")
                                .WithButtonText("Death")
                                .WithHoldTime(0),
                        ]),
                ])
                .WithLabel("Button Page Control")
                .WithButtonText("Enter Page", "Exit Page")
                .WithHoldTime(0.5F);

        private static void Handler(Player player, ButtonInstance button)
        {
            // technically redundant as buttons do not have an initial response.
            if (button.IsInitialResponse)
                return;

            player.Kill("death");
        }
    }
}