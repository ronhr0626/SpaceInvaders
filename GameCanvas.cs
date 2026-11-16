using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SpaceInvaders.Game;

namespace SpaceInvaders;

// Draws the game and runs the game loop. Menus are handled by MainWindow.
public class GameCanvas : Control
{
    // Colors
    public static readonly IBrush AlienABrush = new SolidColorBrush(Color.Parse("#FF5FD7"));
    public static readonly IBrush AlienBBrush = new SolidColorBrush(Color.Parse("#4FD8FF"));
    public static readonly IBrush AlienCBrush = new SolidColorBrush(Color.Parse("#7CFF6B"));
    public static readonly IBrush UfoBrush = new SolidColorBrush(Color.Parse("#FF4D4D"));
    public static readonly IBrush PlayerBrush = new SolidColorBrush(Color.Parse("#3DFF8B"));
    private static readonly IBrush BunkerBrush = new SolidColorBrush(Color.Parse("#2FD46A"));
    private static readonly IBrush PlayerShotBrush = Brushes.White;
    private static readonly IBrush AlienShotBrush = new SolidColorBrush(Color.Parse("#FFE14D"));
    private static readonly IBrush GroundBrush = new SolidColorBrush(Color.Parse("#2FD46A"));
    private static readonly IBrush HudBrush = Brushes.White;
    private static readonly IBrush HudDimBrush = new SolidColorBrush(Color.Parse("#9AA5B1"));
    private static readonly IBrush OverlayBrush = new SolidColorBrush(Color.Parse("#B0000000"));
    private static readonly IBrush StarBrush = new SolidColorBrush(Color.Parse("#55FFFFFF"));

    // Effect colors, picked by Effect.ColorIndex
    private static readonly IBrush[] EffectBrushes =
    {
        AlienABrush, AlienBBrush, AlienCBrush, UfoBrush, AlienShotBrush, PlayerBrush,
    };

    private static readonly Typeface HudFont = new("Consolas", FontStyle.Normal, FontWeight.Bold);

    private GameWorld? _world;  // null while the menu is shown
    private readonly InputState _input = new();
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _clock = new();
    private readonly List<Point> _stars = new();
    private TopLevel? _topLevel;
    private int _highScore;

    // True while a game is being played (not in menus)
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    // Raised once when the game ends. The int is the final score.
    public event Action<int>? GameOver;

    public GameCanvas()
    {
        // A fixed starry background
        var rng = new Random(7);
        for (int i = 0; i < 80; i++)
            _stars.Add(new Point(rng.NextDouble() * GameConfig.Width, rng.NextDouble() * GameConfig.GroundY));

        // Crisp pixel edges when scaled up
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

        // ~60 frames per second
        _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (_, _) => GameLoop();
    }

    public void StartNewGame()
    {
        _world = new GameWorld();
        _highScore = HighScoreStore.Best;
        _input.Clear();
        IsPaused = false;
        IsRunning = true;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Listen for keys on the whole window, not just this control
        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.AddHandler(KeyDownEvent, HandleKeyDown, RoutingStrategies.Tunnel);
        _topLevel?.AddHandler(KeyUpEvent, HandleKeyUp, RoutingStrategies.Tunnel);

        _clock.Start();
        _timer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _timer.Stop();
        _topLevel?.RemoveHandler(KeyDownEvent, HandleKeyDown);
        _topLevel?.RemoveHandler(KeyUpEvent, HandleKeyUp);
        base.OnDetachedFromVisualTree(e);
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        // In the menus the keys go to the buttons and the name box instead
        if (!IsRunning) return;

        _input.Press(e.PhysicalKey);
        e.Handled = true; // stops Space from clicking hidden buttons
    }

    private void HandleKeyUp(object? sender, KeyEventArgs e)
    {
        _input.Release(e.PhysicalKey);
        if (IsRunning) e.Handled = true;
    }

    private void GameLoop()
    {
        // Delta time: seconds since the last frame (capped so a lag spike doesn't teleport things)
        double dt = Math.Min(_clock.Elapsed.TotalSeconds, 0.05);
        _clock.Restart();

        if (IsRunning && _world != null)
        {
            if (_input.PausePressed) IsPaused = !IsPaused;

            if (!IsPaused) _world.Update(dt, _input);

            if (_world.IsOver)
            {
                IsRunning = false;
                _input.Clear();
                GameOver?.Invoke(_world.Score);
            }
        }

        _input.EndFrame();
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Black, new Rect(Bounds.Size));

        // Scale the 640x360 game screen to fit the window, centered
        double scale = Math.Min(Bounds.Width / GameConfig.Width, Bounds.Height / GameConfig.Height);
        double offsetX = (Bounds.Width - GameConfig.Width * scale) / 2;
        double offsetY = (Bounds.Height - GameConfig.Height * scale) / 2;

        using (context.PushTransform(Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offsetX, offsetY)))
        using (context.PushClip(new Rect(0, 0, GameConfig.Width, GameConfig.Height)))
        {
            DrawStars(context);
            if (_world == null) return; // menu: only the stars, the menu is drawn on top by XAML

            DrawHud(context, _world);
            DrawBunkers(context, _world);
            DrawAliens(context, _world);
            DrawUfo(context, _world);
            DrawPlayer(context, _world);
            DrawShots(context, _world);
            DrawEffects(context, _world);
            DrawGround(context, _world);
            DrawBanner(context, _world);
            if (IsPaused) DrawPause(context);
        }
    }

    private void DrawStars(DrawingContext context)
    {
        foreach (var star in _stars)
            context.FillRectangle(StarBrush, new Rect(star.X, star.Y, 1, 1));
    }

    private void DrawHud(DrawingContext context, GameWorld world)
    {
        DrawText(context, $"SCORE {world.Score:D6}", 10, HudBrush, 10, 4);
        DrawText(context, $"HI {Math.Max(_highScore, world.Score):D6}", 10, HudBrush, GameConfig.Width / 2, 4, center: true);
        DrawText(context, $"WAVE {world.Level}", 10, HudBrush, GameConfig.Width - 70, 4);
    }

    private static void DrawBunkers(DrawingContext context, GameWorld world)
    {
        foreach (var bunker in world.Bunkers)
            for (int r = 0; r < Bunker.RowCount; r++)
                for (int c = 0; c < Bunker.Columns; c++)
                    if (bunker.IsSolid(r, c))
                        context.FillRectangle(BunkerBrush,
                            new Rect(bunker.X + c * Bunker.CellSize, bunker.Y + r * Bunker.CellSize, Bunker.CellSize, Bunker.CellSize));
    }

    private static void DrawAliens(DrawingContext context, GameWorld world)
    {
        foreach (var alien in world.Fleet.Alive)
        {
            var brush = alien.Type switch { 0 => AlienABrush, 1 => AlienBBrush, _ => AlienCBrush };
            SpriteRenderer.Draw(context, alien.Frames[world.Fleet.Frame], alien.X, alien.Y, Sprites.AlienPixel, brush);
        }
    }

    private static void DrawUfo(DrawingContext context, GameWorld world)
    {
        if (world.Ufo != null)
            SpriteRenderer.Draw(context, Sprites.Ufo, world.Ufo.X, world.Ufo.Y, Sprites.UfoPixel, UfoBrush);
    }

    private static void DrawPlayer(DrawingContext context, GameWorld world)
    {
        if (world.PlayerVisible)
            SpriteRenderer.Draw(context, Sprites.Player, world.Player.X, world.Player.Y, Sprites.PlayerPixel, PlayerBrush);
    }

    private static void DrawShots(DrawingContext context, GameWorld world)
    {
        if (world.PlayerShot != null)
        {
            var s = world.PlayerShot;
            context.FillRectangle(PlayerShotBrush, new Rect(s.X, s.Y, Bullet.Width, Bullet.Height));
        }

        foreach (var s in world.AlienShots)
        {
            // Zig-zag look: two small offset pieces
            context.FillRectangle(AlienShotBrush, new Rect(s.X - 1, s.Y, Bullet.Width, Bullet.Height / 2));
            context.FillRectangle(AlienShotBrush, new Rect(s.X + 1, s.Y + Bullet.Height / 2, Bullet.Width, Bullet.Height / 2));
        }
    }

    private void DrawEffects(DrawingContext context, GameWorld world)
    {
        foreach (var effect in world.Effects)
        {
            var brush = EffectBrushes[Math.Clamp(effect.ColorIndex, 0, EffectBrushes.Length - 1)];
            if (effect.Text != null)
                DrawText(context, effect.Text, 10, brush, effect.X, effect.Y);
            else
                SpriteRenderer.Draw(context, Sprites.Explosion, effect.X, effect.Y, Sprites.AlienPixel, brush);
        }
    }

    private void DrawGround(DrawingContext context, GameWorld world)
    {
        context.FillRectangle(GroundBrush, new Rect(0, GameConfig.GroundY, GameConfig.Width, 1));

        // Lives: a number and small ships under the ground line
        DrawText(context, world.Lives.ToString(), 10, HudBrush, 10, GameConfig.GroundY + 5);
        int icons = Math.Min(world.Lives - 1, 6);
        for (int i = 0; i < icons; i++)
            SpriteRenderer.Draw(context, Sprites.Player, 26 + i * 22, GameConfig.GroundY + 7, 1.2, PlayerBrush);

        DrawText(context, "P = PAUSE", 9, HudDimBrush, GameConfig.Width - 70, GameConfig.GroundY + 6);
    }

    private void DrawBanner(DrawingContext context, GameWorld world)
    {
        if (world.BannerTimer <= 0) return;
        DrawText(context, world.BannerText, 22, HudBrush, GameConfig.Width / 2, 200, center: true);
    }

    private void DrawPause(DrawingContext context)
    {
        context.FillRectangle(OverlayBrush, new Rect(0, 0, GameConfig.Width, GameConfig.Height));
        DrawText(context, "PAUSED", 28, HudBrush, GameConfig.Width / 2, 140, center: true);
        DrawText(context, "Press P to continue", 11, HudDimBrush, GameConfig.Width / 2, 180, center: true);
    }

    // Draws text in game coordinates. center = x is the middle of the text.
    private void DrawText(DrawingContext context, string text, double size, IBrush brush, double x, double y, bool center = false)
    {
        var formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, HudFont, size, brush);
        double drawX = center ? x - formatted.Width / 2 : x;
        context.DrawText(formatted, new Point(drawX, y));
    }
}
