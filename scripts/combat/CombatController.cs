using Godot;
using System;
using System.Collections.Generic;

public enum RoundPhase { PreRound, Betting, Combat}
public partial class CombatController : Node2D
{
	[Export] private int startingDraw = 5;

	// Lanes
	[Export] private CardLane playerLane;
	[Export] private CardLane enemyLane;

	// UI Refs
	[Export] private Button StartBettingButton;

	private List<CardData> playerDeck = new();
	private List<CardData> enemyDeck = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		StartCombatEncounter();
	}

	public void StartCombatEncounter()
	{
		StartBettingButton.Pressed += OnClickStartBetting;
		EnterPreRound();
	}

	private void OnClickStartBetting()
	{

	}

	private void EnterPreRound()
	{
		
	}

}
