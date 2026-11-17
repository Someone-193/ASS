namespace ASS.Features.Attributes
{
    using ASS.Features.Templates;

    public class HeaderTransformAttribute : ParserTransformAttribute<HeaderTemplate>
    {
        public HeaderTransformAttribute(string? label = null, string? hint = null)
        {
            Label = label;
            Hint = hint;
        }

        public string? Label { get; }

        public string? Hint { get; }

        public override void Apply(HeaderTemplate template)
        {
            if (Label is not null)
                template.WithLabel(Label);

            if (Hint is not null)
                template.WithHint(Hint);
        }
    }
}