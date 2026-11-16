namespace SpaceInvaders.Game;

// The mystery ship that sometimes flies across the top. Worth bonus points.
public class Ufo
{
    private const double Speed = 70;

    public double X { get; private set; }
    public double Y => GameConfig.UfoY;
    public int Direction { get; }   // 1 = moving right, -1 = moving left
    public int Points { get; }

    public double Width => Sprites.Ufo.Width * Sprites.UfoPixel;
    public double Height => Sprites.Ufo.Height * Sprites.UfoPixel;

    public Hitbox Hitbox => new(X, Y, Width, Height);

    public Ufo(int direction, int points)
    {
        Direction = direction;
        Points = points;
        X = direction > 0 ? -Width : GameConfig.Width; // start just outside the screen
    }

    public void Update(double dt) => X += Speed * Direction * dt;

    public bool IsOffScreen => Direction > 0 ? X > GameConfig.Width : X + Width < 0;
}
