using Drawie.Backend.Core.ColorsImpl.Paintables;

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
}

public enum MicronStyleType
{
    Unknown,
    Section
}