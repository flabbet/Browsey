using System.Globalization;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Text;

namespace Browsey.Micron;

public class MicronParser
{
    public static MicronDocument Parse(string source)
    {
        MicronDocument document = new MicronDocument();

        MicronElement? activeElement = null;
        MicronStyle activeStyle = new MicronStyle();
        MicronBlock activeBlock = new MicronBlock();
        string elementBuffer = "";
        var enumerator = StringInfo.GetTextElementEnumerator(source);

        while (enumerator.MoveNext())
        {
            string element = enumerator.GetTextElement();
            if (element is "\n" or "\r\n")
            {
                if (activeElement != null)
                {
                    activeBlock.AddElement(activeElement);
                }

                document.AddElement(activeBlock);
                elementBuffer = "";
                activeBlock = new MicronBlock();

                activeElement = null;
                activeStyle = activeStyle with { ActiveType = MicronStyleType.Unknown };
                continue;
            }

            bool elementBufferCleared = false;
            bool updatedStyle = false;
            if (ShouldUpdateStyle(elementBuffer, element))
            {
                string elemBufferBefore = elementBuffer;
                activeStyle = TryUpdateStyle(activeStyle, element, ref elementBuffer);
                updatedStyle = true;
                if (elemBufferBefore != elementBuffer)
                {
                    elementBufferCleared = true;
                    if(activeElement != null)
                        activeBlock.AddElement(activeElement);
                    activeElement = null;
                }
            }
            else if (activeElement == null)
            {
                activeElement = TryDetectElement(element);
            }

            if (!updatedStyle)
            {
                if (activeElement is MicronInline inline)
                {
                    inline.Text += element;
                }
                else
                {
                    TryUpdateElement(activeElement, element, elementBuffer);
                }
            }

            activeElement?.Style = activeStyle;
            if (!elementBufferCleared)
                elementBuffer += element;
        }

        return document;
    }

    private static bool ShouldUpdateStyle(string blockBuffer, string element)
    {
        if (blockBuffer.Length == 0)
        {
            return element is ">" or "<" or "`";
        }

        return blockBuffer[0] == '`' || element == "`";
    }

    private static MicronElement? TryDetectElement(string element)
    {
        if (element == "-")
        {
            return new MicronSeparator();
        }

        return new MicronInline();
    }

    private static void TryUpdateElement(MicronElement? activeElement, string element, string buffer)
    {
        if (activeElement is MicronSeparator separator)
        {
            if (buffer.Length == 1)
            {
                separator.Separator = element;
            }
        }
    }

    private static MicronStyle TryUpdateStyle(MicronStyle style, string element, ref string elementBuffer)
    {
        MicronStyle newStyle = style;
        if (style.ActiveType == MicronStyleType.Unknown)
        {
            if (element == ">")
            {
                newStyle.ActiveType = MicronStyleType.Section;
                newStyle.Depth = 0;
            }

            if (element == "`")
            {
                newStyle.ActiveType = MicronStyleType.Formatting;
                elementBuffer = "`";
            }
        }
        else if (style.ActiveType == MicronStyleType.Section)
        {
            if (element == ">")
            {
                newStyle.Depth++;
            }

            if (element == "<")
            {
                newStyle.Depth = 0;
                newStyle.ActiveType = MicronStyleType.Unknown;
            }
        }
        else if (style.ActiveType == MicronStyleType.Formatting)
        {
            if (element == "c")
            {
                newStyle.TextAlignment = TextAlign.Center;
                elementBuffer = "";
                newStyle.ActiveType = MicronStyleType.Unknown;
            }
            else if (element == "r")
            {
                newStyle.TextAlignment = TextAlign.Right;
                elementBuffer = "";
                newStyle.ActiveType = MicronStyleType.Unknown;
            }
            else if (element == "a")
            {
                newStyle.TextAlignment = TextAlign.Left;
                elementBuffer = "";
                newStyle.ActiveType = MicronStyleType.Unknown;
            }
            else if (element == "*")
            {
                newStyle.FontSlant = newStyle.FontSlant == FontStyleSlant.Italic
                    ? FontStyleSlant.Upright
                    : FontStyleSlant.Italic;
                elementBuffer = "";
                newStyle.ActiveType = MicronStyleType.Unknown;
            }
            else if (element == "!")
            {
                newStyle.FontWeight = newStyle.FontWeight == FontStyleWeight.Bold
                    ? FontStyleWeight.Normal
                    : FontStyleWeight.Bold;
                elementBuffer = "";
                newStyle.ActiveType = MicronStyleType.Unknown;
            }
            else if (element is "F" or "B" || elementBuffer.ElementAtOrDefault(1) is 'F' or 'B')
            {
                if (elementBuffer.Length == 4)
                {
                    var color = Color.Parse($"#{elementBuffer[2]}{elementBuffer[3]}{element}");
                    if (elementBuffer.ElementAtOrDefault(1) == 'F')
                    {
                        newStyle.TextColor = color;
                    }
                    else
                    {
                        newStyle.BackgroundColor = color;
                    }
                    elementBuffer = "";
                    newStyle.ActiveType = MicronStyleType.Unknown;
                }
            }
            else if (element is "f" or "b" or "`")
            {
                newStyle.ActiveType = MicronStyleType.Unknown;
                if (element is "`" or "f")
                {
                    newStyle.TextColor = Colors.White;
                }

                if (element is "`" or "b")
                {
                    newStyle.BackgroundColor = Colors.Transparent;
                }

                elementBuffer = "";
            }
            else
            {
                newStyle.ActiveType = MicronStyleType.Unknown;
            }
        }

        return newStyle;
    }
}