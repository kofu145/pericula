using Godot;
using System;

public partial class PreStagePanel : PanelContainer
{
    [Export] RichTextLabel stageName;
    [Export] RichTextLabel stageNumber;
    [Export] Button startRoundButton;
    int _stageID;

    public void Initialize(int id)
    {
        _stageID = id;

        // TODO: Set stage name based on id
        stageName.Text = "[wave]Enemy[/wave]";
        stageNumber.Text = "Round " + id;

        if (id > StageManager.Instance.CurrentStageID)
        {
            DisableAsLocked();
        }
        else if (id < StageManager.Instance.CurrentStageID)
        {
            DisableAsCompleted();
        }
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