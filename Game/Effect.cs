namespace SpaceInvaders.Game;

// A short visual effect: an explosion, or a floating score text
public class Effect
{
    public double X { get; }
    public double Y { get; }
    public string? Text { get; }      // null = explosion sprite
    public int ColorIndex { get; }    // which color the renderer should use
    public double TimeLeft { get; private set; }

    public Effect(double x, double y, double duration, int colorIndex, string? text = null)
    {
        X = x;
        Y = y;
        TimeLeft = duration;
        ColorIndex = colorIndex;
        Text = text;
    }

    public void Update(double dt) => TimeLeft -= dt;
    public bool IsDone => TimeLeft <= 0;
}
