namespace ASS.ASSML.Features.Factories
{
    using System.Globalization;
    using System.Xml.Linq;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    public class Button
    {
        internal static IASSObject ParseButton(XElement el, object? context)
        {
            ButtonTemplate button = ButtonTemplate.Create(el.Key);

            if (el.Attribute("Label") is { } label)
                button.WithLabel(label.Value);

            if (el.Attribute("HoldTime") is { } hold)
                button.WithHoldTime(float.Parse(hold.Value, CultureInfo.InvariantCulture));

            return button;
        }
    }
}