using Godot;
using System;
using System.Collections.Generic;

public enum RoundPhase { PreRound, Betting, Showdown }
public partial class CombatController : Node2D
{
	[Export] private int startingDraw = 5;

	// Scene refs
	[Export] private CombatEntityController player;
	[Export] private CombatEntityController enemy;
	[Export] private BetController betController;

	// test data
	[Export] public CardData testCardData;

	// runtime refs
	private RoundPhase currentPhase = RoundPhase.PreRound;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var dummyDeck = new List<CardData>() { testCardData, testCardData, testCardData, testCardData, testCardData };
		player.Initialize(dummyDeck);
		enemy.Initialize(dummyDeck);

		betController.OnBetPhaseEnd += EndBetPhase;

		StartCombatEncounter();
	}

	public void StartCombatEncounter()
	{
		// StartBettingButton.Pressed += OnClickStartBetting;
		StartPrePhase();
	}

	private void OnClickStartBetting()
	{

	}

	private void StartPrePhase()
	{
		currentPhase = RoundPhase.PreRound;

		// draw starting hand for each lane
		player.StartRound(startingDraw);
		enemy.StartRound(startingDraw);
	}

	public void StartBetPhase()
	{
		currentPhase = RoundPhase.Betting;
		betController.BeginPhase();
	}

	private void EndBetPhase()
	{
		// check if either side has folded
		// StartShowdownPhase(); if nobody folded
	}

	private void StartShowdownPhase()
	{
		currentPhase = RoundPhase.Showdown;
	}
}
