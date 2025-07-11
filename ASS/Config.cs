namespace ASS
{
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
    }
}