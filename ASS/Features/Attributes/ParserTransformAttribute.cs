namespace ASS.Features.Attributes
{
    using System;

    using ASS.Features.Interfaces;

    /// <summary>
    /// An attribute for transforming parsers at will.
    /// </summary>
    /// <typeparam name="T">The type of the child. Used by <see cref="Apply"/>.</typeparam>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public abstract class ParserTransformAttribute<T> : Attribute, IParserTransform
    {
        public abstract void Apply(T template);
    }
}