namespace SpaceInvaders.Game;

// One alien in the fleet. Type 0 = top row, 1 = middle rows, 2 = bottom rows.
public class Alien
{
    public int Type { get; }
    public double X { get; set; }
    public double Y { get; set; }
    public bool Alive { get; set; } = true;

    public Alien(int type, double x, double y)
    {
        Type = type;
        X = x;
        Y = y;
    }

    public Sprite[] Frames => Type switch
    {
        0 => Sprites.AlienA,
        1 => Sprites.AlienB,
        _ => Sprites.AlienC,
    };

    public int Points => Type switch
    {
        0 => 30,
        1 => 20,
        _ => 10,
    };

    public double Width => Frames[0].Width * Sprites.AlienPixel;
    public double Height => Frames[0].Height * Sprites.AlienPixel;

    public Hitbox Hitbox => new(X, Y, Width, Height);
}
