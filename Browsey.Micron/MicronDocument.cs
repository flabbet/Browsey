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

public record MicronElement;

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

public record MicronElementStyle
{
    public float XOffset { get; set; }
    public Paintable Background { get; set; }
    public Paintable Foreground { get; set; }
}

public record MicronSection : MicronElement
{
    public string? Header { get; set; }
    public int Level { get; set; }
}