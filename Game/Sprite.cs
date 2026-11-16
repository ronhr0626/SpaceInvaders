using System.Collections.Generic;

namespace SpaceInvaders.Game;

// A tiny pixel-art image made from text. '#' = filled pixel, '.' = empty.
public sealed class Sprite
{
    public int Width { get; }
    public int Height { get; }

    // Horizontal runs of filled pixels: (row, start column, length).
    // Drawing one rectangle per run is much faster than one per pixel.
    public IReadOnlyList<(int Row, int Col, int Length)> Runs { get; }

    public Sprite(params string[] rows)
    {
        Height = rows.Length;
        Width = rows[0].Length;

        var runs = new List<(int, int, int)>();
        for (int r = 0; r < rows.Length; r++)
        {
            int c = 0;
            while (c < rows[r].Length)
            {
                if (rows[r][c] != '#') { c++; continue; }

                int start = c;
                while (c < rows[r].Length && rows[r][c] == '#') c++;
                runs.Add((r, start, c - start));
            }
        }
        Runs = runs;
    }
}
