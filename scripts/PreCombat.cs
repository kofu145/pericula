using Godot;
using System;

public partial class PreCombat : Control
{
    [Export] PackedScene PreStagePanel;
    [Export] VBoxContainer StageList;
    [Export] RichTextLabel anteCountLabel;

    public override void _Ready()
    {
        base._Ready();
        CreatePreStagePanels();
        anteCountLabel.Text = $"Ante: {StageManager.Instance.CurrentAnte + 1} / {StageManager.Instance.AnteCount}";
    }

    public void CreatePreStagePanels()
    {
        var stageManager = StageManager.Instance;
        for (int i = 0; i < stageManager.EnemiesPerAnte; i++)
        {
            PreStagePanel panel = PreStagePanel.Instantiate<PreStagePanel>();
            StageList.AddChild(panel);
            panel.Initialize(i, stageManager.GetEnemyAtIndex(i));
        }
    }
}
