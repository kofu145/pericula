using Godot;
using System;
using System.Collections.Generic;

public partial class UiOverlay : CanvasLayer
{
    [Export] private Control chips;
    [Export] private Label chipsCountLabel;
    [Export] private Button deckButton;
    [Export] private DeckListView deckListView;

    [Export] private Godot.Collections.Array<string> hideOnScenes;
    [Export] private Godot.Collections.Array<string> hideChipsOnScenes;

    public static UiOverlay Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }

        if (hideOnScenes.Contains(GetTree().CurrentScene?.Name)) Visible = false;

        ChipManager.Instance.OnChipsChanged += UpdateChipsUI;

        deckButton.Pressed += deckListView.OpenDeck;

        // get the full path
        for (int i = 0; i < hideOnScenes.Count; i++)
        {
            var scene = hideOnScenes[i];
            hideOnScenes[i] = $"scenes/{scene}.tscn";
        }
        for (int i = 0; i < hideChipsOnScenes.Count; i++)
        {
            var scene = hideChipsOnScenes[i];
            hideChipsOnScenes[i] = $"scenes/{scene}.tscn";
        }
    }

    public void BindDraw(Button button) => button.Pressed += deckListView.OpenDraw;
    public void BindDiscard(Button button) => button.Pressed += deckListView.OpenDiscard;
    public void Enemy(int index) => deckListView.OpenEnemy(index);

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

    public void Refresh(string sceneName)
    {
        Visible = !hideOnScenes.Contains(sceneName);
        chips.Visible = !hideChipsOnScenes.Contains(sceneName);
    }

    private void UpdateChipsUI(int newAmount)
    {
        chipsCountLabel.Text = newAmount.ToString();
    }
}
