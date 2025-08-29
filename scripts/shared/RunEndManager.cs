using Godot;
using System;

public partial class RunEndManager : Node
{
    public static RunEndManager Instance { get; private set; }
    [Export] private RunEndPanel runEndPanel;

    public override void _Ready()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }
    }

    public void LoseRun()
    {
        runEndPanel.Visible = true;
        runEndPanel.InitializeLoss();
    }

    public void WinRun()
    {
        runEndPanel.Visible = true;
        runEndPanel.InitializeWin();
    }
}
