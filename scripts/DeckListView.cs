using Godot;
using System;
using System.Collections.Generic;


public partial class DeckListView : Control
{
    // Scene refs
    [Export] private PackedScene CardScene;

    // UI refs
    [Export] private Label headerLabel;
    [Export] private ScrollContainer Scroll;
    [Export] private GridContainer Grid;

    // Layout config
    [Export] private Vector2I CardSize = new(260, 360); // pixel size of each card cell
    [Export] private int HGap = 24;
    [Export] private int VGap = 24;

    // runtime refs
    private List<CardBase> _cards = new();

    private enum ViewMode { Closed, Deck, Draw, Discard, ShopRemove, ShopUpgrade, ShopDuplicate }
    private ViewMode currentMode = ViewMode.Closed;

    public void OpenDeck() => Toggle(ViewMode.Deck);
    public void OpenDraw() => Toggle(ViewMode.Draw);
    public void OpenDiscard() => Toggle(ViewMode.Discard);
    public void OpenShopRemove() => Toggle(ViewMode.ShopRemove);
    public void OpenShopUpgrade() => Toggle(ViewMode.ShopUpgrade);
    public void OpenShopDuplicate() => Toggle(ViewMode.ShopDuplicate);


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Grid.AddThemeConstantOverride("h_separation", HGap);
        Grid.AddThemeConstantOverride("v_separation", VGap);

        Resized += UpdateColumns;
        if (Scroll != null) Scroll.Resized += UpdateColumns;

        CallDeferred(nameof(UpdateColumns));
        // playerBattleDeck
        // playerDisc
    }

    private void Toggle(ViewMode mode)
    {
        // already opened
        if (currentMode == mode && Visible == true) { Visible = false; return; }

        currentMode = mode;
        Visible = true;
        Refresh();
    }

    private void Refresh()
    {
        Clear();

        var dm = DeckManager.Instance;

        Godot.Collections.Array<CardData> source = currentMode switch
        {
            ViewMode.Deck => dm.PlayerFullDeck,
            ViewMode.Draw => dm.PlayerDrawPile,
            ViewMode.Discard => dm.playerDisc,
            ViewMode.ShopRemove => dm.PlayerFullDeck,
            ViewMode.ShopDuplicate => dm.PlayerFullDeck,
            ViewMode.ShopUpgrade => dm.GetUpgradableCardsInPlayerDeck(),
            _ => DeckManager.Instance.PlayerFullDeck,
        };

        headerLabel.Text = currentMode switch
        {
            ViewMode.Deck => "Player Deck",
            ViewMode.Draw => "Player Draw Pile",
            ViewMode.Discard => "Player Discard Pile",
            ViewMode.ShopRemove => "Remove a Card",
            ViewMode.ShopUpgrade => "Transmogify a Card",
            ViewMode.ShopDuplicate => "Duplicate a Card",
            _ => ""
        };

        Populate(source);
    }

    private void UpdateColumns()
    {
        // Width available to the grid (inside the ScrollContainer)
        float avail = Scroll?.Size.X ?? Size.X;

        // Each cell takes card width + horizontal gap (except the last column)
        float cell = CardSize.X + HGap;

        // Columns = how many cells fit; clamp to >= 1
        int cols = Mathf.Max(1, Mathf.FloorToInt((avail + HGap) / cell));
        Grid.Columns = cols;
    }

    public void Clear()
    {
        foreach (var c in _cards) c.QueueFree();
        _cards.Clear();
    }

    /// <summary>
    /// Populates the deck view with the given list of cardDatas.
    /// </summary>
    /// <param name="deck"> list of cards to populate the view with</param>
    public void Populate(IEnumerable<CardData> deck)
    {
        Clear();
        foreach (var data in deck) SpawnCard(data);
    }

    private void SpawnCard(CardData data)
    {
        var cardBase = CardScene.Instantiate<CardBase>();
        cardBase.EnableDefaultDrag = false;
        Grid.AddChild(cardBase);
        _cards.Add(cardBase);

        cardBase.Initialize(data);
    }
}
