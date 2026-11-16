using System.Collections.Generic;
using Avalonia.Input;

namespace SpaceInvaders.Game;

// Remembers which keys are held down, and which were pressed this frame.
// PhysicalKey = the key's position on the keyboard, so WASD works on Norwegian keyboards too.
public class InputState
{
    private readonly HashSet<PhysicalKey> _down = new();
    private readonly HashSet<PhysicalKey> _pressedThisFrame = new();

    public void Press(PhysicalKey key)
    {
        if (_down.Add(key)) _pressedThisFrame.Add(key); // only the first press counts, not key repeat
    }

    public void Release(PhysicalKey key) => _down.Remove(key);

    public bool IsDown(PhysicalKey key) => _down.Contains(key);
    public bool WasPressed(PhysicalKey key) => _pressedThisFrame.Contains(key);

    // Called once at the end of every frame
    public void EndFrame() => _pressedThisFrame.Clear();

    public void Clear()
    {
        _down.Clear();
        _pressedThisFrame.Clear();
    }

    public bool Left => IsDown(PhysicalKey.ArrowLeft) || IsDown(PhysicalKey.A);
    public bool Right => IsDown(PhysicalKey.ArrowRight) || IsDown(PhysicalKey.D);
    public bool Fire => IsDown(PhysicalKey.Space) || IsDown(PhysicalKey.ArrowUp) || IsDown(PhysicalKey.W);
    public bool PausePressed => WasPressed(PhysicalKey.P) || WasPressed(PhysicalKey.Escape);
}
