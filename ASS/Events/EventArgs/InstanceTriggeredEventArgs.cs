namespace ASS.Events.EventArgs
{
    using ASS.Events.Interfaces;
    using ASS.Features.Instances;
    using LabApi.Features.Wrappers;

    public class InstanceTriggeredEventArgs(Player player, ASSInstance instance) : IASSEventArgs
    {
        public Player Player { get; set; } = player;

        public ASSInstance Instance { get; set; } = instance;
    }
}