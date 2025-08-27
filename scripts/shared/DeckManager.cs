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

    public void FinishAndReset()
    {
        playerBattleDeck.Clear();
        enemyBattleDeck.Clear();
        playerDisc.Clear();
        enemyDisc.Clear();
        initialized = false;
    }
}
