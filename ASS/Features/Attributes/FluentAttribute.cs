namespace ASS.Features.Attributes
{
    using System;

    public class FluentAttribute : Attribute
    {
        public FluentAttribute()
        {
            DefaultValue = null;
        }

        public FluentAttribute(bool defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public bool? DefaultValue { get; set; }
    }
}