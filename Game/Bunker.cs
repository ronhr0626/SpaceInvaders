using System;

namespace SpaceInvaders.Game;

// A shield the player can hide behind. It is made of small blocks that break when hit.
public class Bunker
{
    public const double CellSize = 2;

    private static readonly string[] Shape =
    {
        "....##########....",
        "..##############..",
        ".################.",
        "##################",
        "##################",
        "##################",
        "##################",
        "#####........#####",
        "####..........####",
        "###............###",
    };

    public static int Columns => Shape[0].Length;
    public static int RowCount => Shape.Length;
    public static double Width => Columns * CellSize;
    public static double Height => RowCount * CellSize;

    private readonly bool[,] _cells = new bool[Shape.Length, Shape[0].Length];

    public double X { get; }
    public double Y { get; }

    public Hitbox Hitbox => new(X, Y, Width, Height);

    public Bunker(double x, double y)
    {
        X = x;
        Y = y;

        for (int r = 0; r < RowCount; r++)
            for (int c = 0; c < Columns; c++)
                _cells[r, c] = Shape[r][c] == '#';
    }

    public bool IsSolid(int row, int col) => _cells[row, col];

    // Checks if a shot hits a solid block. If so, blows a small hole and returns true.
    public bool TryHit(Hitbox shot, bool fromAbove, Random rng)
    {
        if (!Hitbox.Intersects(shot)) return false;

        // Search from the side the shot is coming from
        for (int i = 0; i < RowCount; i++)
        {
            int r = fromAbove ? i : RowCount - 1 - i;
            for (int c = 0; c < Columns; c++)
            {
                if (!_cells[r, c] || !CellBox(r, c).Intersects(shot)) continue;

                Blast(r, c, rng);
                return true;
            }
        }
        return false;
    }

    // Aliens that walk into the bunker erase it
    public void Erase(Hitbox area)
    {
        if (!Hitbox.Intersects(area)) return;

        for (int r = 0; r < RowCount; r++)
            for (int c = 0; c < Columns; c++)
                if (_cells[r, c] && CellBox(r, c).Intersects(area))
                    _cells[r, c] = false;
    }

    private Hitbox CellBox(int r, int c) => new(X + c * CellSize, Y + r * CellSize, CellSize, CellSize);

    // Removes blocks around the hit point, a bit random so holes look ragged
    private void Blast(int row, int col, Random rng)
    {
        for (int r = row - 2; r <= row + 2; r++)
        {
            for (int c = col - 2; c <= col + 2; c++)
            {
                if (r < 0 || r >= RowCount || c < 0 || c >= Columns) continue;

                int distance = Math.Abs(r - row) + Math.Abs(c - col);
                if (distance <= 1 || (distance <= 3 && rng.NextDouble() < 0.5))
                    _cells[r, c] = false;
            }
        }
    }
}
