using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceInvaders.Game;

// The whole group of aliens. They move together: sideways, and one step down at the edge.
// The fewer aliens left, the faster they move.
public class AlienFleet
{
    public const int Columns = 11;
    public const int Rows = 5;

    private const double SpacingX = 24;
    private const double SpacingY = 18;
    private const double StepX = 4;   // sideways step
    private const double StepDown = 8; // drop when hitting the edge

    private readonly int _level;
    private int _direction = 1; // 1 = right, -1 = left
    private double _stepTimer;

    public List<Alien> Aliens { get; } = new();
    public int Frame { get; private set; } // animation frame 0 or 1

    public AlienFleet(int level)
    {
        _level = level;

        double fleetWidth = (Columns - 1) * SpacingX + Sprites.AlienA[0].Width * Sprites.AlienPixel;
        double startX = (GameConfig.Width - fleetWidth) / 2;
        double startY = 48 + Math.Min(level - 1, 6) * 8; // each wave starts a bit lower

        for (int row = 0; row < Rows; row++)
        {
            int type = row == 0 ? 0 : row < 3 ? 1 : 2;
            for (int col = 0; col < Columns; col++)
                Aliens.Add(new Alien(type, startX + col * SpacingX, startY + row * SpacingY));
        }
    }

    public IEnumerable<Alien> Alive => Aliens.Where(a => a.Alive);
    public int AliveCount => Aliens.Count(a => a.Alive);
    public bool IsCleared => AliveCount == 0;

    // Lowest point of the fleet (used to check if they have landed)
    public double Bottom => Alive.Max(a => a.Y + a.Height);

    // Time between steps: shorter on higher levels and when few aliens are left
    private double StepInterval
    {
        get
        {
            double baseInterval = Math.Max(0.30, 0.55 - (_level - 1) * 0.04);
            return Math.Max(0.02, baseInterval * AliveCount / (Columns * Rows));
        }
    }

    public void Update(double dt)
    {
        if (IsCleared) return;

        _stepTimer += dt;
        if (_stepTimer < StepInterval) return;
        _stepTimer = 0;
        Frame = 1 - Frame;

        double left = Alive.Min(a => a.X);
        double right = Alive.Max(a => a.X + a.Width);

        bool hitsRightEdge = _direction > 0 && right + StepX > GameConfig.Width - GameConfig.SidePadding;
        bool hitsLeftEdge = _direction < 0 && left - StepX < GameConfig.SidePadding;

        if (hitsRightEdge || hitsLeftEdge)
        {
            foreach (var alien in Aliens) alien.Y += StepDown;
            _direction = -_direction;
        }
        else
        {
            foreach (var alien in Aliens) alien.X += StepX * _direction;
        }
    }

    // Returns the alien hit by a shot, or null
    public Alien? HitTest(Hitbox shot) => Alive.FirstOrDefault(a => a.Hitbox.Intersects(shot));

    // Only the lowest alien in each column can shoot
    public Alien? PickShooter(Random rng)
    {
        var shooters = Alive
            .GroupBy(a => Math.Round(a.X))
            .Select(column => column.OrderByDescending(a => a.Y).First())
            .ToList();

        return shooters.Count == 0 ? null : shooters[rng.Next(shooters.Count)];
    }
}
