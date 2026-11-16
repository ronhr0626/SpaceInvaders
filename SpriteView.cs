using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SpaceInvaders.Game;

namespace SpaceInvaders;

// Shows one game sprite inside the menus, e.g. <local:SpriteView SpriteName="AlienA" Color="#FF5FD7"/>
public class SpriteView : Control
{
    public static readonly StyledProperty<string> SpriteNameProperty =
        AvaloniaProperty.Register<SpriteView, string>(nameof(SpriteName), "AlienA");

    public static readonly StyledProperty<IBrush?> ColorProperty =
        AvaloniaProperty.Register<SpriteView, IBrush?>(nameof(Color), Brushes.White);

    static SpriteView()
    {
        AffectsRender<SpriteView>(SpriteNameProperty, ColorProperty);
    }

    public string SpriteName
    {
        get => GetValue(SpriteNameProperty);
        set => SetValue(SpriteNameProperty, value);
    }

    public IBrush? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var sprite = Sprites.ByName(SpriteName);

        // Fit the sprite inside the control and center it
        double pixel = System.Math.Min(Bounds.Width / sprite.Width, Bounds.Height / sprite.Height);
        double x = (Bounds.Width - sprite.Width * pixel) / 2;
        double y = (Bounds.Height - sprite.Height * pixel) / 2;

        SpriteRenderer.Draw(context, sprite, x, y, pixel, Color ?? Brushes.White);
    }
}
