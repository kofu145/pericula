using Godot;
using System;

public partial class UiOverlay : CanvasLayer
{
    [Export] private Control chips;
    [Export] private Label chipsCountLabel;
    [Export] private Button deckButton;
    [Export] private DeckListView deckListView;

    public static UiOverlay Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }
        CallDeferred(nameof(Refresh));

        if (GetTree().CurrentScene?.Name == "TitleScreen") Visible = false;

        ChipManager.Instance.OnChipsChanged += UpdateChipsUI;

        deckButton.Pressed += deckListView.OpenDeck;
    }

    public void BindDraw(Button button) => button.Pressed += deckListView.OpenDraw;
    public void BindDiscard(Button button) => button.Pressed += deckListView.OpenDiscard;

    /// <summary>
    /// Opens up a menu of your deck. Each card on the view will have the onClickHandler.
    /// </summary>
    /// <param name="onClickHandler">The-on click function that the base cards will inherit</param>
    public void Remove(Action<CardBase> onClickHandler) => deckListView.OpenShopRemove(onClickHandler);

    /// <summary>
    /// Opens up a menu of your deck. Each card on the view will have the onClickHandler.
    /// </summary>
    /// <param name="onClickHandler">The-on click function that the base cards will inherit</param>
    public void Upgrade(Action<CardBase> onClickHandler) => deckListView.OpenShopUpgrade(onClickHandler);

    /// <summary>
    /// Opens up a menu of your deck. Each card on the view will have the onClickHandler.
    /// </summary>
    /// <param name="onClickHandler">The-on click function that the base cards will inherit</param>
    public void Duplicate(Action<CardBase> onClickHandler) => deckListView.OpenShopDuplicate(onClickHandler);

    public void Refresh()
    {
        GD.Print($"CurrentScene: {GetTree().CurrentScene?.Name}");

        Visible = !(GetTree().CurrentScene?.Name == "TitleScreen");
        chips.Visible = !(GetTree().CurrentScene?.Name == "Combat");
    }

    private void UpdateChipsUI(int newAmount)
    {
        chipsCountLabel.Text = newAmount.ToString();
    }
}
