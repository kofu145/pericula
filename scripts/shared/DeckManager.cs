using Godot;
using System;

public partial class DeckManager : Node
{
    public static readonly int seed = 100000;

    [Export]
    public readonly Deck PlayerDeck;

    [Export]
    public Deck EnemyDeck;

    // Intermediary collections used when actually battling
    private Godot.Collections.Array<CardData> playerBattleDeck;
    private Godot.Collections.Array<CardData> enemyBattleDeck;

    private bool initialized;

    public readonly Random RndGen = new(seed);
    public Godot.Collections.Array<CardData> Hand = new();
    public Godot.Collections.Array<CardData> EnemyHand = new();

    public Godot.Collections.Array<CardData> playerDisc = new();
    public Godot.Collections.Array<CardData> enemyDisc = new();


    public override void _Ready()
    {
        initialized = false;
        Shuffle(true);
        Shuffle(false);
    }

    /// <summary>
    /// Must be called before the start of any round.
    /// </summary>
    public void Initialize()
    {
        initialized = true;
        playerBattleDeck = PlayerDeck.Cards.Duplicate(true);
        enemyBattleDeck = EnemyDeck.Cards.Duplicate(true);
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
    public void FinishAndReset()
    {
        playerBattleDeck.Clear();
        enemyBattleDeck.Clear();
        playerDisc.Clear();
        enemyDisc.Clear();
        initialized = false;
    }
}
