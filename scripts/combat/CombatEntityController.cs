using Godot;
using System.Collections.Generic;

public enum LaneSide { Player, Enemy }

public partial class CombatEntityController : Node
{
	[Export] private LaneSide owner = LaneSide.Player;
	[Export] private CardLane lane;

	private Deck _deck;

	public void Initialize(IEnumerable<CardData> startingDeck)
	{
		_deck = new Deck(startingDeck);
		lane.BindSide(owner);
	}

	public void StartRound(int n)
	{
		var drawn = _deck.Draw(n);
		foreach (var data in drawn) lane.SpawnCard(data);
	}

	public void EndRound() => lane.EndRound();
}
