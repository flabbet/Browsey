using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Text;

namespace Browsey.Micron;

public class MicronDocument
{
    public IReadOnlyList<MicronElement> Elements => elements;

    private List<MicronElement> elements = new List<MicronElement>();

    public void AddElement(MicronElement element)
    {
        elements.Add(element);
    }
}

public record MicronElement
{
    public MicronStyle Style { get; set; }
}

public record MicronBlock : MicronElement
{
    public IReadOnlyList<MicronElement> Elements => elements;

    private List<MicronElement> elements = new List<MicronElement>();

    public void AddElement(MicronElement element)
    {
        elements.Add(element);
    }
}

public record MicronInline : MicronElement
{
    public string Text { get; set; }
}

public record MicronSeparator : MicronElement
{
    public string Separator { get; set; } = "-";
}

public struct MicronStyle
{
    public MicronStyleType ActiveType { get; set; }
    public int Depth { get; set; }
    public TextAlign TextAlignment { get; set; }
    public FontStyleWeight FontWeight { get; set; }
    public FontStyleSlant FontSlant { get; set; }
    public Color TextColor { get; set; }
    public Color BackgroundColor { get; set; }

    public MicronStyle()
    {
        ActiveType = MicronStyleType.Unknown;
        Depth = 0;
        TextAlignment = TextAlign.Left;
        FontWeight = FontStyleWeight.Normal;
        FontSlant = FontStyleSlant.Upright;
        TextColor = Colors.White;
        BackgroundColor = Colors.Transparent;
    }
}

public enum MicronStyleType
{
    Unknown,
    Section,
    Formatting
}