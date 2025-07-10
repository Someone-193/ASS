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

        /// <summary>
        /// Updates a <see cref="PlayerMenu"/> by using the menus <see cref="GroupUpdateHandler"/> to acquire new values.
        /// </summary>
        /// <param name="registerChange">If true, makes a red dot appear next to the owners SSS tab indicating a change.</param>
        /// <param name="ignoreNextResponse">If true, ignores the default response from the updated settings. Useful for avoiding inf loops.</param>
        public void Update(bool registerChange = true, bool ignoreNextResponse = false)
        {
            ASSGroup newGroup = Generator(Owner);

            Current.Settings = newGroup.Settings;
            Current.Priority = newGroup.Priority;
            Current.Viewers = newGroup.Viewers;
            Current.SubGroups = newGroup.SubGroups;

            ASSNetworking.SendToPlayer(Owner, true, registerChange, ignoreNextResponse);
        }

        public void Destroy()
        {
            ASSNetworking.UnregisterGroups([Current], [Owner]);
        }
    }
}