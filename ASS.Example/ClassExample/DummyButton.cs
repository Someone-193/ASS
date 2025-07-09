namespace ASS.Example.ClassExample
{
    using ASS.Settings;
    using ASS.Settings.Inheritors;
    using LabApi.Features.Wrappers;

    public class DummyButton : ASSButton
    {
        public DummyButton(int id, string? buttonLabel = null, string? buttonText = null, float holdTime = 0, string? hint = null)
            : base(id, buttonLabel, buttonText, holdTime, hint)
        {
        }

        protected override void OnPressed(Player player, ASSButton button)
        {
            player.Kill("Hit the funny class button");
        }
    }
}