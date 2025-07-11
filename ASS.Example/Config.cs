namespace ASS.Example
{
    #if EXILED
    using Exiled.API.Interfaces;

    public class Config : IConfig
    #elif LABAPI
    public class Config
    #endif
    {
        #if EXILED
        public bool IsEnabled { get; set; }
        #endif

        public bool Debug { get; set; }
    }
}