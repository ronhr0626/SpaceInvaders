namespace SpaceInvaders.Game;

// A simple rectangle used for collision checks
public readonly record struct Hitbox(double X, double Y, double Width, double Height)
{
    public double Right => X + Width;
    public double Bottom => Y + Height;

    // True if the two rectangles overlap
    public bool Intersects(Hitbox other) =>
        X < other.Right && Right > other.X &&
        Y < other.Bottom && Bottom > other.Y;
}
