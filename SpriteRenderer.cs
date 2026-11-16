using Avalonia;
using Avalonia.Media;
using SpaceInvaders.Game;

namespace SpaceInvaders;

// Draws a text-based Sprite with rectangles
public static class SpriteRenderer
{
    public static void Draw(DrawingContext context, Sprite sprite, double x, double y, double pixelSize, IBrush brush)
    {
        foreach (var (row, col, length) in sprite.Runs)
            context.FillRectangle(brush, new Rect(x + col * pixelSize, y + row * pixelSize, length * pixelSize, pixelSize));
    }
}
