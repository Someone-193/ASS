namespace ASS.Settings
{
    using LabApi.Features.Wrappers;

    public class PlayerMenu
    {
        public PlayerMenu(GroupUpdateHandler generator, Player owner)
        {
            Generator = generator;
            Owner = owner;

            Current = generator(owner);

            ASSNetworking.RegisterGroups([Current], [owner]);
        }

        public delegate ASSGroup GroupUpdateHandler(Player player);

        public GroupUpdateHandler Generator { get; set; }

        public Player Owner { get; set; }

        public ASSGroup Current { get; set; }

        public void Update()
        {
            ASSGroup newGroup = Generator(Owner);

            Current.Settings = newGroup.Settings;
            Current.Priority = newGroup.Priority;
            Current.Viewers = newGroup.Viewers;
            Current.SubGroups = newGroup.SubGroups;

            ASSNetworking.SendToPlayer(Owner);
        }

        public void Destroy()
        {
            ASSNetworking.RegisterGroups([Current], [Owner]);
        }
    }
}