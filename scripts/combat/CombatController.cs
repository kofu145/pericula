using Godot;
using System;
using System.Collections.Generic;

public enum RoundPhase { PreRound, Betting, Combat }
public partial class CombatController : Node2D
{
	[Export] private int startingDraw = 5;

	// Lanes
	[Export] private CombatEntityController player;
	[Export] private CombatEntityController enemy;

	// UI Refs
	[Export] private Button StartBettingButton;

	[Export] public CardData testCardData;

	// runtime refs
	private RoundPhase currentPhase = RoundPhase.PreRound;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var dummyDeck = new List<CardData>() { testCardData, testCardData, testCardData, testCardData, testCardData };
		player.Initialize(dummyDeck);
		enemy.Initialize(dummyDeck);

		StartCombatEncounter();
	}

	public void StartCombatEncounter()
	{
		// StartBettingButton.Pressed += OnClickStartBetting;
		EnterPreRound();
	}

	private void OnClickStartBetting()
	{

	}

	private void EnterPreRound()
	{
		currentPhase = RoundPhase.PreRound;

		// draw starting hand for each lane
		player.StartRound(startingDraw);
		enemy.StartRound(startingDraw);
	}

}
