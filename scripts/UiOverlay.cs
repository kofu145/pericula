using Godot;
using System;

public partial class UiOverlay : CanvasLayer
{
    [Export] private Label chipsCountLabel;
    [Export] private Button deckButton;
    [Export] private DeckListView deckListView;

    public static UiOverlay Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }

        if (GetTree().CurrentScene?.Name == "TitleScreen") Visible = false;

        ChipManager.Instance.OnChipsChanged += UpdateChipsUI;

        deckButton.Pressed += deckListView.OpenDeck;
    }

    public void BindDraw(Button drawButton)
    {
        drawButton.Pressed += deckListView.OpenDraw;
    }

    public void BindDiscard(Button discardButton)
    {
        discardButton.Pressed += deckListView.OpenDiscard;
    }

    public void Refresh()
    {
        Visible = !(GetTree().CurrentScene?.Name == "TitleScreen");
    }

    private void UpdateChipsUI(int newAmount)
    {
        chipsCountLabel.Text = newAmount.ToString();
    }
}
