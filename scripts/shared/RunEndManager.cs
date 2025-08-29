using Godot;
using System;

public partial class RunEndManager : Node
{
    public static RunEndManager Instance { get; private set; }
    [Export] private Panel losePanel;
    [Export] private Panel winPanel;

    public override void _Ready()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }
    }

    public void LoseRun()
    {
        losePanel.Visible = true;
    }

    public void WinRun()
    {

    }
}
