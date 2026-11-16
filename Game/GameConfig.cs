namespace SpaceInvaders.Game;

// All fixed sizes and positions in one place.
// The game is drawn in a 640x360 "virtual" screen that is scaled up to fit the window.
public static class GameConfig
{
    public const double Width = 640;
    public const double Height = 360;

    public const double GroundY = 338;      // the green line at the bottom
    public const double PlayerY = 318;      // top of the player ship
    public const double BunkerY = 282;      // top of the bunkers
    public const double UfoY = 26;          // the mystery ship flies here

    public const double SidePadding = 10;   // how close things may get to the screen edge

    public const int StartLives = 3;
    public const int ExtraLifeEvery = 1500; // points needed for an extra life
}
