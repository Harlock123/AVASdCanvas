using Avalonia;
using Avalonia.Media;
using AVASdCanvas.Models;

namespace AVASdCanvas.Rendering;

/// <summary>
/// Renders a GraphicEntity as a labeled rounded rectangle.
/// </summary>
public static class EntityRenderer
{
    public static void Render(DrawingContext ctx, GraphicEntity entity, bool isHovered)
    {
        var rect = entity.Bounds;
        var cornerRadius = entity.CornerRadius;

        // Fill
        if (entity.Fill is { } fill)
        {
            ctx.DrawRectangle(fill, null, new RoundedRect(rect, cornerRadius));
        }

        // Border
        var strokeThickness = isHovered ? entity.StrokeThickness + 1.5 : entity.StrokeThickness;
        if (entity.Stroke is { } stroke)
        {
            var pen = new Pen(stroke, strokeThickness);
            ctx.DrawRectangle(null, pen, new RoundedRect(rect, cornerRadius));
        }

        // Hover highlight
        if (isHovered)
        {
            var highlightBrush = new SolidColorBrush(Colors.White, 0.2);
            ctx.DrawRectangle(highlightBrush, null, new RoundedRect(rect, cornerRadius));
        }

        // Label
        if (!string.IsNullOrEmpty(entity.Label) && entity.LabelBrush is { } labelBrush)
        {
            var formattedText = new FormattedText(
                entity.Label,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                14,
                labelBrush);

            formattedText.MaxTextWidth = entity.Width - 8;
            formattedText.MaxTextHeight = entity.Height - 4;
            formattedText.TextAlignment = TextAlignment.Center;

            // Center vertically
            double textX = rect.X + 4;
            double textY = rect.Y + (rect.Height - formattedText.Height) / 2;

            ctx.DrawText(formattedText, new Point(textX, textY));
        }
    }
}
