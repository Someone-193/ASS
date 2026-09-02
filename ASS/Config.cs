namespace ASS
{
    using System.ComponentModel;

    #if EXILED
    using Exiled.API.Interfaces;

    public class Config : IConfig
    #elif LABAPI
    public class Config
    #endif
    {
        #if EXILED
        public bool IsEnabled { get; set; } = true;
        #endif

        public bool Debug { get; set; }

        [Description("With most SSS code, you often run into having multiple plugins try to send a player settings multiplie time" +
                     "when they join. With this set to True, ASS will always ensure it's the first plugin sending a player their settings," +
                     "which can resolve conflicts. If set to False, objects like the PlayerMenu might make a players settings not" +
                     "appear visible when they first join.")]
        public bool ForceJoinSyncPriority { get; set; } = true;
    }
}