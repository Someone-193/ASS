namespace ASS.ASSML.Features
{
    using System;
    using System.Xml;
    using System.Xml.Linq;

    public class AssmlException : Exception
    {
        public AssmlException(string message, XElement? element = null)
            : base(element is IXmlLineInfo lineInfo && lineInfo.HasLineInfo()
                ? $"{message} (line {lineInfo.LineNumber})"
                : message)
        {
        }
    }
}