namespace ASS.Features.Templates.Displays
{
    using ASS.Features.Attributes;

    [CanSync]
    public abstract class ASSDisplayTemplate : ASSTemplate
    {
        protected ASSDisplayTemplate(string? key, int? id)
            : base(key, id)
        {
        }
    }
}