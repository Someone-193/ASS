namespace ASS.Features.Commands
{
    using System;
    using CommandSystem;
    using LabApi.Features.Wrappers;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ForceUpdate : ICommand
    {
        public string Command => "ForceUpdate";

        public string Description => "Forces ASS to send you settings";

        public string[] Aliases => [];

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player? p = Player.Get(sender);
            if (p is null)
            {
                response = "Player not found";
                return false;
            }

            ASSNetworking.SendToPlayerFull(p, true, false, true);
            response = "Did the thing.";
            return true;
        }
    }
}