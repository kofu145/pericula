using Godot;
using System;
using System.Collections.Generic;

public enum RoundPhase { PreRound, Betting, Showdown }
public partial class PhaseController : Node2D
{
	[Export] private int startingDraw = 5;
	[Export] private float displayDuration = 1.5f;

	// Scene refs
	[Export] private CombatEntityController player;
	[Export] private CombatEntityController enemy;
	[Export] private BetController betController;

	// UI refs
	[Export] private Label actionLabel;

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
		betController.OnEnemyAction += DisplayEnemyAction;

		StartCombatEncounter();
	}

	public void StartCombatEncounter()
	{
		StartPrePhase();
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

	private void DisplayEnemyAction(BetAction action, int called, int raised)
	{
		string text = action switch
		{
			BetAction.Check => "check",
			BetAction.Fold => "fold",
			BetAction.Call => $"call {called}",
			BetAction.Raise => $"raise {raised}",
			BetAction.CallAndRaise => $"call {called}  raise {raised}",
			_ => ""
		};

		ShowActionLabel(text, displayDuration);
	}

	private async void ShowActionLabel(string text, float duration)
	{
		actionLabel.Text = text;
		actionLabel.Visible = true;

		await ToSignal(GetTree().CreateTimer(duration), "timeout");

		actionLabel.Visible = false;
	}
}
