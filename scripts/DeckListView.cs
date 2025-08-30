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
    [Export] private Button CloseButton;

    // Layout config

    // runtime refs
    private List<CardBase> _cards = new();

    private enum ViewMode { Deck, Draw, Discard, ShopRemove, ShopUpgrade, ShopDuplicate }
    private List<ViewMode> CanClose = new() { ViewMode.Deck, ViewMode.Draw, ViewMode.Discard };

    public void OpenDeck() => Open(ViewMode.Deck);
    public void OpenDraw() => Open(ViewMode.Draw);
    public void OpenDiscard() => Open(ViewMode.Discard);
    public void OpenEnemy(int index) => OpenEnemyPanel(index);
    public void OpenShopRemove(Action<CardBase> onClickHandler) => Open(ViewMode.ShopRemove, onClickHandler);  // should have a function as aa parameter
    public void OpenShopUpgrade(Action<CardBase> onClickHandler) => Open(ViewMode.ShopUpgrade, onClickHandler);
    public void OpenShopDuplicate(Action<CardBase> onClickHandler) => Open(ViewMode.ShopDuplicate, onClickHandler);


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        CloseButton.Pressed += Close;
    }

    private void Open(ViewMode mode, Action<CardBase> onClickHandler = null)
    {
        Visible = true;
        Refresh(mode, onClickHandler);
        CloseButton.Visible = CanClose.Contains(mode);
    }

    private void OpenEnemyPanel(int index)
    {
        Visible = true;
        CloseButton.Visible = true;

        var enemy = StageManager.Instance.GetEnemyAtIndex(index);

        Clear();
        headerLabel.Text = $"{enemy.DisplayName}'s Deck";
        Populate(enemy.deck.Cards, null);
    }

    private void Close()
    {
        Visible = false;
    }

    // add an optional parameter for the function
    private void Refresh(ViewMode mode, Action<CardBase> onClickHandler)
    {
        Clear();

        var dm = DeckManager.Instance;

        Godot.Collections.Array<CardData> source = mode switch
        {
            ViewMode.Deck => dm.PlayerFullDeck,
            ViewMode.Draw => dm.PlayerDrawPile,
            ViewMode.Discard => dm.playerDisc,
            ViewMode.ShopRemove => dm.PlayerFullDeck,
            ViewMode.ShopDuplicate => dm.PlayerFullDeck,
            ViewMode.ShopUpgrade => dm.GetUpgradableCardsInPlayerDeck(),
            _ => DeckManager.Instance.PlayerFullDeck,
        };

        headerLabel.Text = mode switch
        {
            ViewMode.Deck => "Player Deck",
            ViewMode.Draw => "Player Draw Pile",
            ViewMode.Discard => "Player Discard Pile",
            ViewMode.ShopRemove => "Remove a Card",
            ViewMode.ShopDuplicate => "Duplicate a Card",
            ViewMode.ShopUpgrade => "Transmogify a Card",
            _ => ""
        };

        Populate(source, onClickHandler);
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
    public void Populate(IEnumerable<CardData> deck, Action<CardBase> onClickHandler)
    {
        Clear();
        foreach (var data in deck) SpawnCard(data, onClickHandler);
    }

    // add optional parameter on the function
    private void SpawnCard(CardData data, Action<CardBase> onClickHandler)
    {
        var cardBase = CardScene.Instantiate<CardBase>();
        if (onClickHandler != null)
        {
            cardBase.OnLeftClicked += c =>
            {
                onClickHandler(c);
                Close();
            };
        }
        cardBase.EnableDefaultDrag = false;
        Grid.AddChild(cardBase);
        _cards.Add(cardBase);

        cardBase.Initialize(data);
    }
}
