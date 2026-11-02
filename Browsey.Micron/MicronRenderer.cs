using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using Drawie.Rendering;

namespace Browsey.Micron;

using Drawie.Backend.Core.Surfaces;

public sealed class MicronRenderer
{
    private const double PtToPx = 1.3333f;
    private readonly Font _font;
    private readonly Paint _paint;

    private readonly double _padding;
    private readonly double _lineHeight;
    private readonly double _paragraphSpacing;
    private readonly Color backgroundColor1 = Colors.Gray;
    private Dictionary<string, Shader> separatorShaders = new Dictionary<string, Shader>();

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

        canvas.Translate((float)_padding, (float)_padding + (float)_lineHeight);

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
                            int saved = canvas.Save();
                            ApplyStyle(_paint, blockElement.Style, canvas);

                            VecD measured = MeasureElement(blockElement);

                            DrawBackgroundIfNeeded(canvas, measured, blockElement.Style);

                            if (blockElement is MicronInline inline)
                            {
                                DrawText(canvas, inline.Text);
                            }
                            else if (blockElement is MicronSeparator separator)
                            {
                                DrawSeparator(canvas, separator);
                            }

                            canvas.RestoreToCount(saved);
                        }

                        y += _lineHeight;
                        canvas.Translate(0, (float)_lineHeight);

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

    private void DrawBackgroundIfNeeded(Canvas canvas, VecD measured, MicronStyle style)
    {
        if (style.ActiveType == MicronStyleType.Section)
        {
            var original = _paint.Color;
            _paint.Color = backgroundColor1;
            canvas.DrawRect(new RectD(0, -measured.Y, canvas.LocalClipBounds.Width, measured.Y * PtToPx), _paint);
            _paint.Color = original;
        }
    }

    private VecD MeasureElement(MicronElement blockElement)
    {
        if (blockElement is MicronInline inline)
        {
            _font.MeasureText(inline.Text, out RectD bounds, _paint);
            return new VecD(bounds.Width, bounds.Height);
        }

        return VecD.Zero;
    }

    private void ApplyStyle(Paint paint, MicronStyle style, Canvas canvas)
    {
        if (style.Depth > 0)
        {
            canvas.Translate((float)_paragraphSpacing * style.Depth, 0);
        }
    }

    private double DrawText(Canvas canvas, string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        _font.MeasureText(text, out RectD bounds, _paint);

        canvas.DrawText(text, VecD.Zero, _font, _paint);

        return bounds.Width;
    }

    private void DrawSeparator(Canvas canvas, MicronSeparator separator)
    {
        if (!separatorShaders.ContainsKey(separator.Separator))
        {
            var measured = _font.MeasureText(separator.Separator, out RectD bounds, _paint);
            using Texture texture =
                new Texture(new VecI((int)double.Ceiling(measured), (int)double.Ceiling(_lineHeight)));
            texture.DrawingSurface.Canvas.DrawText(separator.Separator, new VecD(0, _font.Size), _paint);
            using Image snapshot = texture.DrawingSurface.Snapshot();
            separatorShaders.Add(separator.Separator,
                Shader.CreateBitmap(Bitmap.FromImage(snapshot), TileMode.Repeat, TileMode.Clamp, Matrix3X3.Identity));
        }

        _paint.Shader = separatorShaders[separator.Separator];
        canvas.DrawRect(0, 0, (float)canvas.LocalClipBounds.Width, (float)_lineHeight, _paint);
        _paint.Shader = null;
    }
}