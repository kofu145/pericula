using Godot;
using System;

public partial class DeckManager : Node
{
	public static readonly int seed = 100000;

	[Export]
	public Deck PlayerDeck;

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
	public static DeckManager Instance { get; private set; }

	public override void _Ready()
	{
		initialized = false;
		Shuffle(true);
		Shuffle(false);
		Instance = this;
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

	public void AddCardByID(int id)
	{
		// need to add lookup table
		// PlayerDeck.Cards.Add();
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

	public void ChangeEnemyDeck(string respath)
	{
		// impl
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

	/// <summary>
	/// Recycles respective discard pile back into respective deck. 
	/// </summary>
	private void RecycleDiscardIntoDraw(bool isPlayer)
	{
		var targetDeck = isPlayer ? playerBattleDeck : enemyBattleDeck;
		var targetDisc = isPlayer ? playerDisc : enemyDisc;

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
