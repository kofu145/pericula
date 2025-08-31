using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class DeckManager : Node
{
    public static readonly int seed = Guid.NewGuid().GetHashCode();

    [Export]
    public Deck PlayerDeck;

    [Export]
    public Deck EnemyDeck;

    private static readonly Dictionary<RarityType, int> RarityRank = new()
    {
        { RarityType.Legendary, 5 },
        { RarityType.Mythic,    4 },
        { RarityType.Rare,      3 },
        { RarityType.Common,    2 },
        { RarityType.Starter,   1 },
        { RarityType.Token,     0 },
    };

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
        Shuffle(true);
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

        Shuffle(true);
        Shuffle(false);
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
            if (data != null && data.Rarity != null && data.Rarity.RarityType != RarityType.Legendary)
                returnList.Add(data);
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
            card.ResetForShowdown();
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

    public void DuplicateCard(CardData card)
    {
        // optional check for if it exists in deck?
        // if (PlayerDeck.Contains(card))

        PlayerDeck.Cards.Add((CardData)card.Duplicate(true));
    }

    public void Remove(CardData cardToRemove)
    {
        PlayerDeck.Cards.Remove(cardToRemove);
    }

    public void Upgrade(CardData card)
    {
        Rarity rarity = card.Rarity;
        if (card.Rarity.RarityType == RarityType.Legendary)
        {
            GD.PrintErr("tried to upgrade a legendary card!");
            return;
        }
        var upgradeTo = (int)rarity.RarityType + 1;

        var targets = Lookup.GetCardsByRarity((RarityType)upgradeTo);
        int target = RndGen.Next(targets.Count);

        Remove(card);
        PlayerDeck.Cards.Add(targets[target]);

    }
    // need to initialize first
    public void ConjureRandomCard()
    {
        int roll = RndGen.Next(100);
        RarityType target = roll < 5 ? RarityType.Legendary : RarityType.Mythic;
        var targets = Lookup.GetCardsByRarity(target);
        PlayerDeck.Cards.Add(targets[RndGen.Next(targets.Count)]);
    }

    // public void Discard(CardData c) => _discard.Add(c);
    // public void DiscardRange(IEnumerable<CardData> cards) => _discard.AddRange(cards);

    public Godot.Collections.Array<CardData> GetOrderedDrawPile()
    {

        if (PlayerDeck == null || PlayerDeck.Cards == null)
            return new Godot.Collections.Array<CardData>();

        // Build a UI-only sorted copy (does not change actual draw order)
        var ordered = playerBattleDeck
            .Where(c => c != null)
            .OrderBy(c => c.Rarity != null ? RarityRank[c.Rarity.RarityType] : int.MaxValue)
            .ThenBy(c => c.Trait) // enum comparison is fine directly
            .ThenBy(c => c.id)
            .ThenBy(c => c.DisplayName ?? string.Empty, StringComparer.Ordinal)
            .ToList();

        var result = new Godot.Collections.Array<CardData>();
        foreach (var c in ordered)
            result.Add(c);
        return result;

    }

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
            cardToAdd.ResetForEncounter();
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
            card.ResetForShowdown();
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
