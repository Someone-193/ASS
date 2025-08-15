namespace ASS.Example.DefaultExample
{
    using ASS.Events.EventArgs;
    using ASS.Features.Settings;

    public class SettingTriggered
    {
        public static void OnSettingTriggered(SettingTriggeredEventArgs ev)
        {
            switch (ev.Setting)
            {
                case ASSButton { Id: -11 } button:
                    ev.Player.Kill($"Hit the funny default button with Id {button.Id}");
                    break;
            }
        }
    }
}