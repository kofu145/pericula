using Godot;
using System;

public partial class PreCombat : Control
{
    [Export] PackedScene PreStagePanel;
    [Export] VBoxContainer StageList;
    [Export] int count = 5;

    public override void _Ready()
    {
        base._Ready();
        CreatePreStagePanels();
    }

    public void CreatePreStagePanels()
    {
        for (int i = 1; i <= count; i++)
        {
            PreStagePanel panel = PreStagePanel.Instantiate<PreStagePanel>();
            StageList.AddChild(panel);
            panel.Initialize(i);
        }
    }
}
