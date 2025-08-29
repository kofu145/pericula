using Godot;
using System;

public partial class PreStagePanel : PanelContainer
{
    [Export] RichTextLabel stageName;
    [Export] RichTextLabel stageNumber;
    [Export] Button startRoundButton;
    int _stageID;

    public void Initialize(int stageNumber, EnemyData data)
    {
        _stageID = stageNumber;

        stageName.Text = $"[wave]{data.DisplayName}[/wave]";
        this.stageNumber.Text = $"Round {stageNumber + 1}";

        if (stageNumber > StageManager.Instance.CurrentStageNumber)
        {
            DisableAsLocked();
        }
        else if (stageNumber < StageManager.Instance.CurrentStageNumber)
        {
            DisableAsCompleted();
        }

        // TODO: Initialize a button that opens a deck preview of the enemy's deck
    }

    public void OnStartRoundClicked()
    {
        StageManager.Instance.BeginStage();
    }

    public void DisableAsCompleted()
    {
        Disable();
        startRoundButton.Text = "DEFEATED";
    }

    public void DisableAsLocked()
    {
        Disable();
        stageName.Text = "???";
    }

    public void Disable()
    {
        Modulate = new Color(0.5f, 0.5f, 0.5f);
        startRoundButton.Disabled = true;

    }
}