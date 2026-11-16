namespace SpaceInvaders.Game;

// All pixel-art in the game, drawn as text. Two frames per alien = simple walking animation.
public static class Sprites
{
    // How big one sprite pixel is in the 640x360 game screen
    public const double AlienPixel = 1.5;
    public const double PlayerPixel = 1.5;
    public const double UfoPixel = 1.3;

    // Top row aliens (30 points)
    public static readonly Sprite[] AlienA =
    {
        new(
            "....###....",
            "..#######..",
            ".##.###.##.",
            "###########",
            ".#.#.#.#.#.",
            "#.#.#.#.#.#"),
        new(
            "....###....",
            "..#######..",
            ".##.###.##.",
            "###########",
            "#.#.#.#.#.#",
            ".#.#.#.#.#."),
    };

    // Middle row aliens (20 points)
    public static readonly Sprite[] AlienB =
    {
        new(
            "..#######..",
            ".#..###..#.",
            "###########",
            "##.#####.##",
            "#..#...#..#",
            "...##.##..."),
        new(
            "..#######..",
            ".#..###..#.",
            "###########",
            "##.#####.##",
            ".#.#...#.#.",
            "##.......##"),
    };

    // Bottom row aliens (10 points)
    public static readonly Sprite[] AlienC =
    {
        new(
            "...#####...",
            ".#########.",
            "##..###..##",
            "###########",
            ".##.#.#.##.",
            "#.........#"),
        new(
            "...#####...",
            ".#########.",
            "##..###..##",
            "###########",
            "##.##.##.##",
            ".#.......#."),
    };

    // The player's ship
    public static readonly Sprite Player = new(
        "......#......",
        ".....###.....",
        "....#####....",
        "..#########..",
        ".###########.",
        "#############",
        "##.##...##.##");

    // The mystery ship at the top
    public static readonly Sprite Ufo = new(
        ".....######.....",
        "...##########...",
        "..############..",
        ".##.##.##.##.##.",
        "################",
        "..###..##..###..",
        "...#........#...");

    // Shown when something is destroyed
    public static readonly Sprite Explosion = new(
        "#...#.#...#",
        ".#..#.#..#.",
        "..#.....#..",
        "##.......##",
        "..#.....#..",
        ".#..#.#..#.",
        "#...#.#...#");

    // Used by the menu to show sprites by name
    public static Sprite ByName(string name) => name switch
    {
        "AlienA" => AlienA[0],
        "AlienB" => AlienB[0],
        "AlienC" => AlienC[0],
        "Player" => Player,
        "Ufo" => Ufo,
        _ => Explosion,
    };
}
