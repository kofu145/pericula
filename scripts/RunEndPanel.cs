using Godot;
using System;

public partial class RunEndPanel : CanvasLayer
{
    [Export] private RichTextLabel header;
    [Export] private RichTextLabel description;

    public void InitializeWin()
    {
        header.Text = "You Won!";
        description.Text = $"You earned a total of: {ChipManager.Instance.ChipsEarned} Chips\n"
        + $"You spent a total of: {ChipManager.Instance.ChipsUsed} Chips";
    }

    public void InitializeLoss()
    {
        header.Text = "You Lost";
        description.Text = $"You earned a total of: {ChipManager.Instance.ChipsEarned} Chips\n"
        + $"You spent a total of: {ChipManager.Instance.ChipsUsed} Chips";
    }

    private void ReturnToTitleScreen()
    {
        SceneManager.ChangeSceneToFile("TitleScreen");
        Visible = false;
    }
}
