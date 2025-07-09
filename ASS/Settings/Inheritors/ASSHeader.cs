namespace ASS.Settings.Inheritors
{
    using System;
    using UserSettings.ServerSpecific;

    public class ASSHeader : ASSBase
    {
        public ASSHeader(string? label, string? hint = null)
        {
            Label = label;
            Hint = hint;
        }

        internal override Type SSSType { get; } = typeof(SSGroupHeader);

        internal override ASSBase Copy() => new ASSHeader(Label, Hint);
    }
}