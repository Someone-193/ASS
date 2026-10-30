namespace ASS.Features.Templates
{
    using System;

    using ASS.Features.Instances;
    using ASS.Features.Interfaces;

    using LabApi.Features.Wrappers;

    using UserSettings.ServerSpecific;

    public delegate void InstanceHandler<in T>(Player player, T instance)
        where T : ASSInstance;

    public abstract class ASSTemplate : IASSObject
    {
        protected ASSTemplate(string? key, int? id)
        {
            Key = key ?? string.Empty;
            OverrideId = id;
        }

        public string Key { get; set; }

        public int? OverrideId { get; set; }

        public string? Label { get; set; }

        public string? Hint { get; set; }

        public byte CollectionId { get; set; } = byte.MaxValue;

        public IASSContainer? Parent { get; set; }

        /// <summary>
        /// Gets the <see cref="ServerSpecificSettingBase.UserResponseMode"/> of the setting. Acquisition implies a response when the setting is received and Change implies a response whenever the setting is triggered.
        /// </summary>
        public abstract ServerSpecificSettingBase.UserResponseMode ResponseMode { get; }

        internal abstract Type SSSType { get; }

        public override string ToString() => $"({Key}) {{{Label ?? "NULL"}}} [{Hint ?? "NULL"}] <{SSSType.Name}>";

        internal abstract ASSInstance CreateInstance(Player viewer, int id);

        internal abstract void RemoveInstance(Player viewer);
    }
}