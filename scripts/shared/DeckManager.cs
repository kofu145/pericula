using Godot;
using System;
using System.Collections;

public partial class DeckManager : Node
{
    public static readonly int seed = 100000;

    [Export]
    public Deck PlayerDeck;

    [Export]
    public Deck EnemyDeck;

    // Intermediary collections used when actually battling
    private Godot.Collections.Array<CardData> playerBattleDeck = new();
    private Godot.Collections.Array<CardData> enemyBattleDeck = new();

    private bool initialized;

    public readonly Random RndGen = new(seed);
    // ====================================
    // public APIs
    // ====================================
    public Godot.Collections.Array<CardData> Hand = new();
    public Godot.Collections.Array<CardData> EnemyHand = new();

    public Godot.Collections.Array<CardData> playerDisc = new();
    public Godot.Collections.Array<CardData> enemyDisc = new();


    public Godot.Collections.Array<CardData> PlayerDrawPile => playerBattleDeck;
    public Godot.Collections.Array<CardData> PlayerFullDeck => PlayerDeck?.Cards;


    public static DeckManager Instance { get; private set; }

    public override void _Ready()
    {
        initialized = false;
        Shuffle(false);
        foreach (var card in PlayerDeck.Cards)
        {
            card.Initialize();
        }
        foreach (var card in EnemyDeck.Cards)
        {
            card.Initialize();
        }
        Instance = this;
    }

    public void StartNewRun(Deck playerDeck)
    {
        PlayerDeck = (Deck)playerDeck.Duplicate(true);
    }

    /// <summary>
    /// Must be called before the start of any round.
    /// </summary>
    public void Initialize()
    {
        initialized = true;
        EnemyDeck = StageManager.Instance.GetCurrentEnemyDeck();
        foreach (var card in EnemyDeck.Cards)
        {
            card.Initialize();
        }
        CloneTempDeck(PlayerDeck.Cards, playerBattleDeck);
        CloneTempDeck(EnemyDeck.Cards, enemyBattleDeck);
    }

    public void AddCardByID(int id)
    {
        PlayerDeck.Cards.Add(Lookup.GetCardByID(id));
    }

    public bool RemoveCardWithID(int id)
    {
        int idxToRemove = -1;
        for (int i = 0; i < PlayerDeck.Cards.Count; i++)
        {
            if (PlayerDeck.Cards[i].id == id)
                idxToRemove = id;
        }
        if (idxToRemove == -1)
            return false;

        PlayerDeck.Cards.RemoveAt(idxToRemove);
        return true;
    }

    public Godot.Collections.Array<CardData> GetUpgradableCardsInPlayerDeck()
    {
        Godot.Collections.Array<CardData> returnList = new();
        foreach (var data in PlayerDeck.Cards)
            if (data.Rarity != "Legendary") returnList.Add(data);
        return returnList;
    }

    /// <summary>
    /// Adds a random assortment of n cards to <seealso cref="Hand"/>.
    /// </summary>
    /// <param name="n">The number of cards to draw.</param>
    /// <param name="isPlayer">The corresponding deck to draw from - true is player, false is enemy.</param>
    public void Draw(int n, bool isPlayer)
    {
        if (!initialized) return;
        var targetList = isPlayer ? Hand : EnemyHand;
        var targetDeck = isPlayer ? playerBattleDeck : enemyBattleDeck;
        var targetDisc = isPlayer ? playerDisc : enemyDisc;

        for (int i = 0; i < n; i++)
        {
            if (targetDeck.Count == 0)
            {
                if (targetDisc.Count == 0) break;
                RecycleDiscardIntoDraw(isPlayer);
            }

            var lastIndex = targetDeck.Count - 1;
            var c = targetDeck[lastIndex];
            targetDeck.RemoveAt(lastIndex);
            targetList.Add(c);
        }
    }

    /// <summary>
    /// Adds a card to the <seealso cref="playerDisc"/> and removes it from <seealso cref="Hand"/>.
    /// </summary>
    /// <param name="c">The card data to be added to the discard pile</param>
    /// <param name="isPlayer">The corresponding deck to discard to - true is player, false is enemy</param>
    public void Discard(CardData c, bool isPlayer)
    {
        if (!initialized) return;
        var targetList = isPlayer ? Hand : EnemyHand;
        var targetDisc = isPlayer ? playerDisc : enemyDisc;

        targetList.Remove(c);
        targetDisc.Add(c);
        foreach (var card in targetDisc)
        {
            card.ResetForBattle();
        }
    }

    public void ClearHand(bool isPlayer)
    {
        if (!initialized) return;
        var targetList = isPlayer ? Hand : EnemyHand;
        var targetDisc = isPlayer ? playerDisc : enemyDisc;

        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            var c = targetList[i];
            targetList.Remove(c);
            targetDisc.Add(c);
        }
    }

    // public void Discard(CardData c) => _discard.Add(c);
    // public void DiscardRange(IEnumerable<CardData> cards) => _discard.AddRange(cards);

    public void FinishAndReset()
    {
        playerBattleDeck.Clear();
        enemyBattleDeck.Clear();
        playerDisc.Clear();
        enemyDisc.Clear();
        initialized = false;
    }

    public void PrintData()
    {
        GD.Print($"Player Deck: {PlayerDeck} Player Disc: {playerDisc} Player Hand {Hand},\\n EnemyDeck{enemyBattleDeck}, enemyDisc: {enemyDisc}, EnemyHand: {EnemyHand}");
    }

    private void CloneTempDeck(Godot.Collections.Array<CardData> list, Godot.Collections.Array<CardData> targetList)
    {
        foreach (var card in list)
        {
            var cardToAdd = (CardData)card.Duplicate(true);
            targetList.Add(cardToAdd);
            cardToAdd.Initialize();
            //card.id = RndGen.Next(100);
        }
    }

    /// <summary>
    /// Recycles respective discard pile back into respective deck. 
    /// </summary>
    private void RecycleDiscardIntoDraw(bool isPlayer)
    {
        var targetDeck = isPlayer ? playerBattleDeck : enemyBattleDeck;
        var targetDisc = isPlayer ? playerDisc : enemyDisc;

        foreach (var card in targetDisc)
        {
            card.ResetForBattle();
        }
        targetDeck.AddRange(targetDisc);
        targetDisc.Clear();
        Shuffle(isPlayer);
    }

    /// <summary>
    /// Carryover shuffle method from previous implementation - shuffles a respective deck.
    /// </summary>
    /// <param name="isPlayer">Determines which deck to shuffle - true: player, false: enemy.</param>
    private void Shuffle(bool isPlayer)
    {
        if (!initialized) return;
        var targetDeck = isPlayer ? playerBattleDeck : enemyBattleDeck;
        for (int i = 0; i < targetDeck.Count; i++)
        {
            int j = RndGen.Next(i + 1);
            (targetDeck[i], targetDeck[j]) = (targetDeck[j], targetDeck[i]);
        }
    }
}
