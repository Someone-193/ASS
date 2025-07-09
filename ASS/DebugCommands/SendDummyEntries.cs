namespace ASS.DebugCommands
{
    using System;
    using ASS.Settings;
    using ASS.Settings.Inheritors;
    using CommandSystem;
    using LabApi.Features.Wrappers;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SendDummyEntries : ICommand
    {
        public string Command => "SendDummyEntries";

        public string[] Aliases { get; } = [];

        public string Description => "Sends you some default settings";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player? user = Player.Get(sender);
            if (user == null)
            {
                response = "Only players can run this command";
                return false;
            }

            ASSNetworking.SendToPlayer(user, [new ASSHeader("Header 1"), new ASSHeader("Header 2")]);

            response = "Sent Dummy Entries";
            return true;
        }
    }
}