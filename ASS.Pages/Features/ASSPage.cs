namespace ASS.Pages.Features
{
    using System.Collections.Generic;
    using System.Linq;

    using ASS.Events.EventArgs;
    using ASS.Features;
    using ASS.Features.Containers;
    using ASS.Features.Interfaces;

    using LabApi.Features.Wrappers;

    using UserSettings.ServerSpecific;

    public abstract class ASSPage : IASSContainer
    {
        protected const string ControlKey = "ASS.Pages Page Controller";

        protected ASSPage(string key) => Key = key;

        public static ASSGroup MainPage { get; } = ASSGroup
            .Create("Main Page")
            .WithChildren(ASSNetworking.Objects.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key));

        public static DropdownPage MainPageController { get; } = DropdownPage
            .Create("ASS.Pages.MainPage")
            .WithLabel("Pages")
            .WithEntryType(SSDropdownSetting.DropdownEntryType.HybridLoop)
            .AddPage("Main Page", MainPage);

        public static Dictionary<Player, Stack<ASSPage>> SelectedPage { get; } = new();

        public string Key { get; }

        public IASSContainer? Parent { get; set; }

        protected HashSet<Player> Opened { get; } = [];

        public abstract IEnumerable<IASSObject> GetChildren(Player viewer);

        internal static void OnObjectRegistered(ObjectRegisteredEventArgs ev) => MainPage.AddChild(ev.Object);

        internal static void OnObjectUnregistered(ObjectUnregisteredEventArgs ev) => MainPage.RemoveChild(ev.Object);
    }
}