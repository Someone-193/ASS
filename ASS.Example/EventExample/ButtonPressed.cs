namespace ASS.Example.EventExample
{
    using ASS.Features.Settings;
    using LabApi.Features.Wrappers;

    public class ButtonPressed
    {
        public static void OnButtonPressed(Player sender, ASSButton button)
        {
            if (button.Id is -10)
            {
                sender.Kill("Hit the funny event button");
            }
        }
    }
}