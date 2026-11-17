namespace ASS.Features.Attributes
{
    using ASS.Features.Templates;

    public class TwoButtonTransformAttribute : ParserTransformAttribute<TwoButtonsTemplate>
    {
        public TwoButtonTransformAttribute(string? label = null, string? leftOption = null, string? rightOption = null, bool defaultRightSelected = false, string? hint = null)
        {
            Label = label;

            LeftOption = leftOption;
            RightOption = rightOption;

            DefaultRightSelected = defaultRightSelected;

            Hint = hint;
        }

        public string? Label { get; }

        public string? LeftOption { get; }

        public string? RightOption { get; }

        public bool DefaultRightSelected { get; }

        public string? Hint { get; }

        public override void Apply(TwoButtonsTemplate template)
        {
            if (Label is not null)
                template.WithLabel(Label);

            if (Hint is not null)
                template.WithHint(Hint);

            if (LeftOption is not null)
                template.WithLeftOption(LeftOption);

            if (RightOption is not null)
                template.WithRightOption(RightOption);

            template.WithDefaultRightSelected(DefaultRightSelected);
        }
    }
}