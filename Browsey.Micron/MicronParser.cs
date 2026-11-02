using System.Globalization;

namespace Browsey.Micron;

public class MicronParser
{
    public static MicronDocument Parse(string source)
    {
        MicronDocument document = new MicronDocument();

        MicronElement? activeElement = null;
        MicronStyle activeStyle = new MicronStyle();
        MicronBlock activeBlock = new MicronBlock();
        string blockBuffer = "";
        var enumerator = StringInfo.GetTextElementEnumerator(source);

        while(enumerator.MoveNext())
        {
            string element = enumerator.GetTextElement();
            if (element is "\n" or "\r\n")
            {
                if (activeElement != null)
                {
                    activeBlock.AddElement(activeElement);
                }

                document.AddElement(activeBlock);
                blockBuffer = "";
                activeBlock = new MicronBlock();

                activeElement = null;
                continue;
            }

            if (ShouldUpdateStyle(blockBuffer, element))
            {
                activeStyle = TryUpdateStyle(activeStyle, element);
            }
            else if (activeElement == null)
            {
                activeElement = TryDetectElement(element);
            }

            if (activeElement is MicronInline inline)
            {
                inline.Text += element;
            }

            activeElement?.Style = activeStyle;
        }

        return document;
    }

    private static bool ShouldUpdateStyle(string blockBuffer, string element)
    {
        if (blockBuffer.Length == 0)
        {
            return element == ">";
        }

        return false;
    }

    private static MicronElement? TryDetectElement(string element)
    {
        return new MicronInline();
    }

    private static MicronStyle TryUpdateStyle(MicronStyle style, string element)
    {
        MicronStyle newStyle = style;
        if (style.ActiveType == MicronStyleType.Unknown)
        {
            if (element == ">")
            {
                newStyle.ActiveType = MicronStyleType.Section;
                newStyle.Depth = 0;
            }
        }
        else if (style.ActiveType == MicronStyleType.Section)
        {
            if(element == ">")
            {
                newStyle.Depth++;
            }
        }

        return newStyle;
    }
}