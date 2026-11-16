namespace SpaceInvaders.Game;

// A shot from the player (goes up) or from an alien (goes down)
public class Bullet
{
    public const double Width = 2;
    public const double Height = 6;

    public double X { get; private set; }
    public double Y { get; private set; }
    public double SpeedY { get; }        // negative = up, positive = down
    public bool FromPlayer => SpeedY < 0;

    public Hitbox Hitbox => new(X, Y, Width, Height);

    public Bullet(double centerX, double y, double speedY)
    {
        X = centerX - Width / 2;
        Y = y;
        SpeedY = speedY;
    }

    public void Update(double dt) => Y += SpeedY * dt;

    public bool IsOffScreen => Y + Height < 0 || Y > GameConfig.GroundY;
}
