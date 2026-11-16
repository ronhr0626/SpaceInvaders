using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SpaceInvaders.Game;

// One line in the top 10 list
public record ScoreEntry(string Name, int Score);

// One line as shown in the menus: three columns so the numbers line up
public record HighScoreRow(string Place, string Name, string Score);

// Saves and loads the top 10 list as JSON.
// The file lives in AppData, so it is never pushed to GitHub by accident.
public static class HighScoreStore
{
    public const int MaxEntries = 10;
    public const int MaxNameLength = 12;

    private static string FilePath
    {
        get
        {
            // AppData on Windows. If it can't be found, fall back to the game's own folder.
            string root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(root)) root = AppContext.BaseDirectory;
            return Path.Combine(root, "SpaceInvadersData", "highscores.json");
        }
    }

    public static List<ScoreEntry> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new List<ScoreEntry>();

            var list = JsonSerializer.Deserialize<List<ScoreEntry>>(File.ReadAllText(FilePath));
            return (list ?? new List<ScoreEntry>())
                .OrderByDescending(e => e.Score)
                .Take(MaxEntries)
                .ToList();
        }
        catch
        {
            return new List<ScoreEntry>(); // broken file = start with an empty list
        }
    }

    public static int Best => Load().FirstOrDefault()?.Score ?? 0;

    // True if the score is good enough for the list
    public static bool Qualifies(int score)
    {
        if (score <= 0) return false;
        var list = Load();
        return list.Count < MaxEntries || score > list[^1].Score;
    }

    // Adds a score, saves the file and returns the new list
    public static List<ScoreEntry> Add(string name, int score)
    {
        name = name.Trim();
        if (name.Length == 0) name = "PLAYER";
        if (name.Length > MaxNameLength) name = name[..MaxNameLength];

        var list = Load();
        list.Add(new ScoreEntry(name.ToUpperInvariant(), score));
        list = list.OrderByDescending(e => e.Score).Take(MaxEntries).ToList();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            // Saving failed (no access to the folder). The game keeps running; the list just isn't saved.
        }
        return list;
    }

    // Makes 10 rows for the menus, with "-" for empty places
    public static List<HighScoreRow> Format(List<ScoreEntry> list)
    {
        var rows = new List<HighScoreRow>();
        for (int i = 0; i < MaxEntries; i++)
        {
            string place = $"{i + 1}.";
            rows.Add(i < list.Count
                ? new HighScoreRow(place, list[i].Name, list[i].Score.ToString())
                : new HighScoreRow(place, "-", ""));
        }
        return rows;
    }
}
