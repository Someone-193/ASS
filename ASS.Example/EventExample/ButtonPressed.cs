namespace ASS.Example.EventExample
{
    using ASS.Events.EventArgs;

    public class ButtonPressed
    {
        public static void OnButtonPressed(ButtonPressedEventArgs ev)
        {
            if (ev.Button.Id is -10)
            {
                ev.Player.Kill("Hit the funny event button");
            }
        }
    }
}