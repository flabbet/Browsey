using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using Drawie.Rendering;

namespace Browsey.Micron;

using Drawie.Backend.Core.Surfaces;

public sealed class MicronRenderer
{
    private readonly Font _font;
    private readonly Paint _paint;

    private readonly double _padding;
    private readonly double _lineHeight;
    private readonly double _paragraphSpacing;

    public MicronRenderer(
        Font font,
        Paint paint,
        double padding = 24,
        double lineHeight = 22,
        double paragraphSpacing = 12)
    {
        _font = font;
        _paint = paint;
        _padding = padding;
        _lineHeight = lineHeight;
        _paragraphSpacing = paragraphSpacing;
    }

    public void Render(TextureFramebuffer framebuffer, MicronDocument document)
    {
        Canvas? canvas = framebuffer.Canvas;
        if (canvas == null)
            return;

        double x = _padding;
        double y = _padding + _lineHeight;

        canvas.Save();

        try
        {
            foreach (var micronElement in document.Elements)
            {
                switch (micronElement)
                {
                    case MicronBlock block:
                    {
                        foreach (var blockElement in block.Elements)
                        {
                            if (blockElement is MicronInline inline)
                            {
                                DrawText(canvas, inline.Text, x, y);
                            }
                        }

                        y += _lineHeight;

                        break;
                    }
                }
            }
        }
        finally
        {
            canvas.Restore();
        }

        canvas.Flush();
    }

    private double DrawText(Canvas canvas, string text, double x, double y)
    {
        if (string.IsNullOrEmpty(text))
            return x;

        _font.MeasureText(text, out RectD bounds, _paint);

        canvas.DrawText(text, new VecD(x, y), _font, _paint);

        return x + bounds.Width;
    }
}