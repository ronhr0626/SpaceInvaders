using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceInvaders.Game;

// All the game rules. Knows nothing about drawing or windows.
public class GameWorld
{
    private const double PlayerShotSpeed = -320;
    private const double AlienShotSpeed = 120;
    private const double RespawnTime = 0.5;
    private const double GameOverDelay = 1.5;
    private const double BannerTime = 2.0;

    private static readonly int[] UfoPoints = { 50, 100, 150, 300 };

    private readonly Random _rng = new();
    private double _respawnTimer;    // > 0 while the player is exploding
    private double _gameOverTimer;   // > 0 while the last explosion plays
    private double _alienFireTimer;
    private double _ufoTimer;
    private int _nextExtraLife = GameConfig.ExtraLifeEvery;

    public Player Player { get; } = new();
    public AlienFleet Fleet { get; private set; }
    public List<Bunker> Bunkers { get; } = new();
    public Bullet? PlayerShot { get; private set; }   // only one player shot at a time, like the original
    public List<Bullet> AlienShots { get; } = new();
    public Ufo? Ufo { get; private set; }
    public List<Effect> Effects { get; } = new();

    public int Score { get; private set; }
    public int Lives { get; private set; } = GameConfig.StartLives;
    public int Level { get; private set; } = 1;
    public bool IsOver { get; private set; }

    // Big text in the middle of the screen ("WAVE 2"), shown while BannerTimer > 0
    public string BannerText { get; private set; } = "GET READY";
    public double BannerTimer { get; private set; } = BannerTime;

    public bool PlayerVisible => _respawnTimer <= 0 && _gameOverTimer <= 0 && !IsOver;

    public GameWorld()
    {
        Fleet = new AlienFleet(Level);
        ResetBunkers();
        ResetUfoTimer();
        ResetAlienFireTimer();
    }

    public void Update(double dt, InputState input)
    {
        if (IsOver) return;

        UpdateEffects(dt);

        // Last explosion before game over
        if (_gameOverTimer > 0)
        {
            _gameOverTimer -= dt;
            if (_gameOverTimer <= 0) IsOver = true;
            return;
        }

        // "GET READY" / "WAVE 2": everything waits
        if (BannerTimer > 0)
        {
            BannerTimer -= dt;
            return;
        }

        // Player is exploding: everything waits, then the ship comes back
        if (_respawnTimer > 0)
        {
            _respawnTimer -= dt;
            if (_respawnTimer <= 0) Player.Reset();
            return;
        }

        UpdatePlayer(dt, input);
        UpdatePlayerShot(dt);
        UpdateFleet(dt);
        if (_gameOverTimer > 0) return; // the aliens landed

        UpdateAlienShots(dt);
        UpdateUfo(dt);
        CheckExtraLife();

        if (Fleet.IsCleared) StartNextWave();
    }

    private void UpdatePlayer(double dt, InputState input)
    {
        Player.Update(dt, input);

        if (input.Fire && PlayerShot == null)
            PlayerShot = new Bullet(Player.CenterX, Player.Y - Bullet.Height, PlayerShotSpeed);
    }

    private void UpdatePlayerShot(double dt)
    {
        if (PlayerShot == null) return;

        PlayerShot.Update(dt);
        var shot = PlayerShot.Hitbox;

        if (PlayerShot.IsOffScreen)
        {
            PlayerShot = null;
            return;
        }

        // Hit an alien?
        var alien = Fleet.HitTest(shot);
        if (alien != null)
        {
            alien.Alive = false;
            Score += alien.Points;
            Effects.Add(new Effect(alien.X, alien.Y, 0.25, alien.Type));
            PlayerShot = null;
            return;
        }

        // Hit the mystery ship?
        if (Ufo != null && Ufo.Hitbox.Intersects(shot))
        {
            Score += Ufo.Points;
            Effects.Add(new Effect(Ufo.X, Ufo.Y, 1.0, 3, Ufo.Points.ToString()));
            Ufo = null;
            ResetUfoTimer();
            PlayerShot = null;
            return;
        }

        // Shot hits an alien shot: both disappear
        var enemyShot = AlienShots.FirstOrDefault(s => s.Hitbox.Intersects(shot));
        if (enemyShot != null)
        {
            AlienShots.Remove(enemyShot);
            Effects.Add(new Effect(PlayerShot.X - 6, PlayerShot.Y, 0.15, 4));
            PlayerShot = null;
            return;
        }

        // Hit a bunker?
        if (Bunkers.Any(b => b.TryHit(shot, fromAbove: false, _rng)))
            PlayerShot = null;
    }

    private void UpdateFleet(double dt)
    {
        Fleet.Update(dt);
        if (Fleet.IsCleared) return;

        // Aliens walking through the bunkers break them
        foreach (var alien in Fleet.Alive)
            foreach (var bunker in Bunkers)
                bunker.Erase(alien.Hitbox);

        // The aliens reached the player: game over at once
        if (Fleet.Bottom >= GameConfig.PlayerY)
        {
            Lives = 0;
            KillPlayer();
            return;
        }

        // Shooting
        _alienFireTimer -= dt;
        if (_alienFireTimer <= 0)
        {
            ResetAlienFireTimer();

            int maxShots = Math.Min(2 + Level, 6);
            var shooter = AlienShots.Count < maxShots ? Fleet.PickShooter(_rng) : null;
            if (shooter != null)
                AlienShots.Add(new Bullet(shooter.X + shooter.Width / 2, shooter.Y + shooter.Height, AlienShotSpeed + Level * 8));
        }
    }

    private void UpdateAlienShots(double dt)
    {
        for (int i = AlienShots.Count - 1; i >= 0; i--)
        {
            var shot = AlienShots[i];
            shot.Update(dt);

            if (shot.IsOffScreen)
            {
                Effects.Add(new Effect(shot.X - 6, GameConfig.GroundY - 8, 0.15, 4));
                AlienShots.RemoveAt(i);
            }
            else if (Bunkers.Any(b => b.TryHit(shot.Hitbox, fromAbove: true, _rng)))
            {
                AlienShots.RemoveAt(i);
            }
            else if (shot.Hitbox.Intersects(Player.Hitbox))
            {
                KillPlayer();
                return;
            }
        }
    }

    private void UpdateUfo(double dt)
    {
        if (Ufo == null)
        {
            _ufoTimer -= dt;
            if (_ufoTimer <= 0 && Fleet.AliveCount > 6)
                Ufo = new Ufo(_rng.Next(2) == 0 ? 1 : -1, UfoPoints[_rng.Next(UfoPoints.Length)]);
            return;
        }

        Ufo.Update(dt);
        if (Ufo.IsOffScreen)
        {
            Ufo = null;
            ResetUfoTimer();
        }
    }

    private void UpdateEffects(double dt)
    {
        foreach (var effect in Effects) effect.Update(dt);
        Effects.RemoveAll(e => e.IsDone);
    }

    private void KillPlayer()
    {
        Effects.Add(new Effect(Player.X + 4, Player.Y, RespawnTime, 5));
        Lives = Math.Max(0, Lives - 1);
        PlayerShot = null;
        AlienShots.Clear();

        if (Lives == 0)
            _gameOverTimer = GameOverDelay;
        else
            _respawnTimer = RespawnTime;
    }

    private void StartNextWave()
    {
        Level++;
        Fleet = new AlienFleet(Level);
        ResetBunkers();
        PlayerShot = null;
        AlienShots.Clear();
        Ufo = null;
        ResetUfoTimer();
        Player.Reset();

        BannerText = $"WAVE {Level}";
        BannerTimer = BannerTime;
    }

    private void CheckExtraLife()
    {
        if (Score < _nextExtraLife) return;

        Lives++;
        _nextExtraLife += GameConfig.ExtraLifeEvery;
        Effects.Add(new Effect(Player.X - 10, Player.Y - 20, 1.2, 5, "1UP"));
    }

    private void ResetBunkers()
    {
        Bunkers.Clear();
        for (int i = 1; i <= 4; i++)
        {
            double centerX = GameConfig.Width / 5 * i;
            Bunkers.Add(new Bunker(centerX - Bunker.Width / 2, GameConfig.BunkerY));
        }
    }

    private void ResetUfoTimer() => _ufoTimer = 15 + _rng.NextDouble() * 10;

    private void ResetAlienFireTimer()
    {
        double baseTime = Math.Max(0.35, 1.0 - (Level - 1) * 0.08);
        _alienFireTimer = baseTime * (0.6 + _rng.NextDouble() * 0.8);
    }
}
