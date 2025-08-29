using Godot;
using System;
using System.Collections.Generic;


public partial class DeckListView : Control
{
    // Scene refs
    [Export] private PackedScene CardScene;

    // UI refs
    [Export] private ScrollContainer Scroll;
    [Export] private GridContainer Grid;

    // Layout config
    [Export] private Vector2I CardSize = new(260, 360); // pixel size of each card cell
    [Export] private int HGap = 24;
    [Export] private int VGap = 24;

    // runtime refs
    private List<CardBase> _cards = new();

    private enum ViewMode { Closed, Deck, Draw, Discard }
    private ViewMode currentMode = ViewMode.Closed;

    public void OpenDeck() => Toggle(ViewMode.Deck);
    public void OpenDraw() => Toggle(ViewMode.Draw);
    public void OpenDiscard() => Toggle(ViewMode.Discard);


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

        Godot.Collections.Array<CardData> source = currentMode switch
        {
            ViewMode.Deck => DeckManager.Instance.PlayerFullDeck,
            ViewMode.Draw => DeckManager.Instance.PlayerDrawPile,
            ViewMode.Discard => DeckManager.Instance.playerDisc,
            _ => DeckManager.Instance.PlayerFullDeck,
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
