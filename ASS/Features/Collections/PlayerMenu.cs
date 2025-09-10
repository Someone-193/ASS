namespace ASS.Features.Collections
{
    using System;
    using LabApi.Features.Wrappers;
    using UserSettings.ServerSpecific;

    public class PlayerMenu
    {
        private bool dirty;

        public PlayerMenu(GroupUpdateHandler generator, Player owner)
        {
            Generator = generator;
            Owner = owner;

            Current = generator(owner);

            ASSNetworking.RegisterGroups([Current], [owner]);
        }

        public delegate ASSGroup GroupUpdateHandler(Player player);

        /// <summary>
        /// Gets or sets a value indicating whether this PlayerMenu will call <see cref="DirtyAction"/> when they open their SSS menu.
        /// </summary>
        /// <remarks>Particularly useful if you have a menu that can change often, but has no keybinds / settings that need to be read unless their menu is open, and you need to update it without causing lag.</remarks>
        public bool Dirty
        {
            get => dirty;
            set
            {
                // if value changes
                if (value ^ dirty)
                {
                    if (value)
                        ServerSpecificSettingsSync.ServerOnStatusReceived += OnReceivedReport;
                    else
                        ServerSpecificSettingsSync.ServerOnStatusReceived -= OnReceivedReport;
                }

                dirty = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Action"/> called when this <see cref="PlayerMenu"/> is <see cref="Dirty"/> and the <see cref="Owner"/> opens their SSS tab.
        /// </summary>
        /// <remarks>
        /// Example: DirtyAction = (menu, _, _) => menu.Update();
        /// <br/><br/>
        /// Dirty will be set to false when this is called.
        /// <br/>
        /// This will automatically update the PlayerMenu when the owner opens up their SSS tab, and can be customized to do other things.
        /// </remarks>
        public Action<PlayerMenu, Player, SSSUserStatusReport>? DirtyAction { get; set; }

        public GroupUpdateHandler Generator { get; set; }

        public Player Owner { get; set; }

        public ASSGroup Current { get; set; }

        /// <summary>
        /// Updates a <see cref="PlayerMenu"/> by using the menus <see cref="GroupUpdateHandler"/> to acquire new values.
        /// </summary>
        /// <param name="registerChange">If true, makes a red dot appear next to the owners SSS tab indicating a change.</param>
        /// <param name="ignoreDefaultResponses">If true, ignores the default response from all settings. Useful for avoiding inf loops.</param>
        /// <param name="onlyGroupsResponses">If true, ignores the default response from everything except the groups settings. Useful for avoiding inf loops.</param>
        public void Update(bool registerChange = true, bool ignoreDefaultResponses = false, bool onlyGroupsResponses = false)
        {
            ASSGroup newGroup = Generator(Owner);

            Current.Settings = newGroup.Settings;
            Current.Priority = newGroup.Priority;
            Current.Viewers = newGroup.Viewers;
            Current.SubGroups = newGroup.SubGroups;

            ASSNetworking.SendToPlayerFull(Owner, true, registerChange, false, ignoreDefaultResponses || onlyGroupsResponses, onlyGroupsResponses ? Current.GetAllSettings().ToArray() : null);
        }

        public void Destroy()
        {
            ASSNetworking.UnregisterGroups([Current], [Owner]);

            Dirty = false;
        }

        private void OnReceivedReport(ReferenceHub hub, SSSUserStatusReport report)
        {
            if (!report.TabOpen || Owner.ReferenceHub != hub)
                return;

            Dirty = false;
            DirtyAction?.Invoke(this, Player.Get(hub), report);
        }
    }
}