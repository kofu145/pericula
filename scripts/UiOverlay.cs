using Godot;
using System;

public partial class UiOverlay : CanvasLayer
{
    [Export] private Label chipsCountLabel;
    public override void _Ready()
    {
        ChipManager.Instance.OnChipsChanged += UpdateChipsUI;
    }

    public void Hide()
    {
        Visible = false;
    }
    public void Show()
    {
        Visible = true;
    }

    private void UpdateChipsUI(int newAmount)
    {
        chipsCountLabel.Text = newAmount.ToString();
    }
}
