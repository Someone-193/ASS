namespace ASS.Example.DefaultExample
{
    using ASS.Settings;
    using ASS.Settings.Inheritors;
    using LabApi.Features.Wrappers;

    public class SettingTriggered
    {
        public static void OnSettingTriggered(Player sender, ASSBase setting)
        {
            switch (setting)
            {
                case ASSButton { Id: -11 } button:
                    sender.Kill($"Hit the funny default button with Id {button.Id}");
                    break;
            }
        }
    }
}