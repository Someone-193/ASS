namespace ASS.Features.Settings
{
    using System;

    using Mirror;

    #if EXILED
    using Exiled.API.Features.Core.UserSettings;
    #endif

    using UserSettings.ServerSpecific;

    public class ASSHeader : ASSBase
    {
        [Obsolete("Headers now require Ids, this ctor uses either the label to generate an Id, or a completely random Id")]
        public ASSHeader(string? label = null, string? hint = null)
        {
            Id = label?.GetStableHashCode() ?? UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            Label = label;
            Hint = hint;
        }

        public ASSHeader(int id, string? label = null, bool reducedPadding = false, string? hint = null)
        {
            Id = id;
            Label = label;
            ReducedPadding = reducedPadding;
            Hint = hint;
        }

        /// <summary>
        /// Gets or sets a value indicating whether ... something to do with reduced padding.
        /// </summary>
        /// <remarks>
        /// Cant automatically sync cuz NW moment.
        /// </remarks>
        public bool ReducedPadding { get; set; }

        public override ServerSpecificSettingBase.UserResponseMode ResponseMode => ServerSpecificSettingBase.UserResponseMode.None;

        internal override Type SSSType { get; } = typeof(SSGroupHeader);

        public static implicit operator ASSHeader(SSGroupHeader header) => new(header.SettingId, header.Label, header.ReducedPadding, header.HintDescription);

        public static implicit operator SSGroupHeader(ASSHeader header) => new(header.Id, header.Label, header.ReducedPadding, header.Hint);

        #if EXILED
        public static implicit operator ASSHeader(HeaderSetting header) => new(header.Id, header.Label, header.ReducedPaddling, header.HintDescription);

        public static implicit operator HeaderSetting(ASSHeader header) => new(header.Id, header.Label, header.Hint, header.ReducedPadding);
        #endif

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.WriteBool(ReducedPadding);
        }

        internal override ASSBase Copy() => new ASSHeader(Id, Label, ReducedPadding, Hint);
    }
}