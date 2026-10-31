namespace Browsey.Micron;

public class MicronParser
{
    public static MicronDocument Parse(string source)
    {
        MicronDocument document = new MicronDocument();

        MicronElement? activeElement = null;
        MicronBlock activeBlock = new MicronBlock();

        foreach (var c in source)
        {
            if (c == '\n')
            {
                if (activeElement != null)
                {
                    activeBlock.AddElement(activeElement);
                }

                document.AddElement(activeBlock);
                activeBlock = new MicronBlock();

                activeElement = null;
                continue;
            }

            if (activeElement == null)
            {
                activeElement = TryDetectElement(c);
            }

            if (activeElement is MicronSection section)
            {
                if (c == '>')
                {
                    section.Level++;
                    continue;
                }

                section.Header += c;
            }

            if (activeElement is MicronInline inline)
            {
                inline.Text += c;
            }
        }

        return document;
    }

    private static MicronElement? TryDetectElement(char c)
    {
        if (c == '>')
        {
            return new MicronSection();
        }

        return null;
    }
}