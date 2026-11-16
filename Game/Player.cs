namespace SpaceInvaders.Game;

// The ship the player controls at the bottom of the screen
public class Player
{
    private const double Speed = 150; // pixels per second

    public double Width => Sprites.Player.Width * Sprites.PlayerPixel;
    public double Height => Sprites.Player.Height * Sprites.PlayerPixel;

    public double X { get; private set; }
    public double Y => GameConfig.PlayerY;

    public Hitbox Hitbox => new(X, Y, Width, Height);
    public double CenterX => X + Width / 2;

    public Player() => Reset();

    // Puts the ship back in the middle (after losing a life)
    public void Reset() => X = (GameConfig.Width - Width) / 2;

    public void Update(double dt, InputState input)
    {
        if (input.Left) X -= Speed * dt;
        if (input.Right) X += Speed * dt;

        // Stay inside the screen
        double min = GameConfig.SidePadding;
        double max = GameConfig.Width - GameConfig.SidePadding - Width;
        if (X < min) X = min;
        if (X > max) X = max;
    }
}
