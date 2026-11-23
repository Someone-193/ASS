namespace ASS.Features.Commands
{
    using System;
    using CommandSystem;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ASSDebug : ICommand
    {
        public string Command => "assdebug";

        public string Description => "Toggles the Debug config for ASS.";

        public string[] Aliases { get; } = [];

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Config? cfg = Main.Instance?.Config;
            if (cfg is null)
            {
                response = "Config is not loaded? Double check ASS is installed properly.";
                return false;
            }

            cfg.Debug = !cfg.Debug;
            response = $"[ASS]: Debug has been set to {cfg.Debug}!";
            return true;
        }
    }
}