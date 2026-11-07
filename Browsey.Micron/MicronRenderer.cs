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
    private readonly Paint _richTextPaint;

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
        _richTextPaint = new Paint();
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
                        RichText? lastRichText = null;
                        for (var i = 0; i < block.Elements.Count; i++)
                        {
                            var blockElement = block.Elements[i];
                            int saved = canvas.Save();
                            VecD measured = MeasureElement(blockElement);

                            ApplyStyle(_paint, blockElement.Style, canvas);

                            DrawBackgroundIfNeeded(canvas, measured, blockElement.Style);

                            if (blockElement is MicronInline inline)
                            {
                                bool nextIsInline = block.Elements.Count - 1 >= i + 1 &&
                                                    block.Elements[i + 1] is MicronInline;

                                lastRichText ??= new RichText();
                                lastRichText.AddInline(ToRichTextInline(inline, blockElement.Style));

                                if (!nextIsInline)
                                {
                                    var style = blockElement.Style;
                                    if (style.TextAlignment == TextAlign.Center)
                                    {
                                        canvas.Translate(
                                            (float)canvas.LocalClipBounds.Width / 2 -
                                            (float)lastRichText.MeasureBounds().Width / 2f, 0);
                                    }
                                    else if (style.TextAlignment == TextAlign.Right)
                                    {
                                        canvas.Translate(
                                            (float)canvas.LocalClipBounds.Width -
                                            (float)lastRichText.MeasureBounds().Width, 0);
                                    }

                                    lastRichText.Paint(canvas, VecD.Zero, _richTextPaint);
                                }
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

    private TextInline ToRichTextInline(MicronInline inline, MicronStyle style)
    {
        return new TextInline(inline.Text,
            new FontData(_font.Family)
                { Edging = _font.Edging, Size = _font.Size, Weight = style.FontWeight, Slant = style.FontSlant })
        {
            Alignment = style.TextAlignment,
            Fill = true,
            FillPaintable = style.TextColor,
        };
    }

    private void DrawBackgroundIfNeeded(Canvas canvas, VecD measured, MicronStyle style)
    {
        Color color = style.ActiveType == MicronStyleType.Section ? backgroundColor1 : style.BackgroundColor;
        if (color.A > 0)
        {
            int saved = canvas.Save();
            canvas.Translate(-canvas.TotalMatrix.TransX + (float)_padding, 0);
            var original = _paint.Color;
            _paint.Color = color;
            canvas.DrawRect(new RectD(0, -measured.Y, canvas.LocalClipBounds.Width, measured.Y * PtToPx), _paint);
            _paint.Color = original;
            canvas.RestoreToCount(saved);
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

    private void DrawSeparator(Canvas canvas, MicronSeparator separator)
    {
        int saved = canvas.Save();
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

        canvas.Translate(-canvas.TotalMatrix.TransX + (float)_padding, 0);
        _paint.Shader = separatorShaders[separator.Separator];
        canvas.DrawRect(0, 0, (float)canvas.LocalClipBounds.Width, (float)_lineHeight, _paint);
        _paint.Shader = null;
        canvas.RestoreToCount(saved);
    }
}