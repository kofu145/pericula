using Godot;
using System;
using System.Collections.Generic;

public enum RoundPhase { PreRound, Betting, Showdown }
public partial class PhaseController : Node2D
{
    [Export] private int startingDraw = 5;
    [Export] private float displayDuration = 1.5f;
    // TODO: temp implementation
    [Export] private int startingChips = 100;
    // minimum starting bet, will be increased by singleton instance as run progresses
    [Export] private int currentMinimumBuyIn = 10;

    [Export] private int enemyChips = 100;

    // Scene refs
    [Export] private CombatController combatManager;
    [Export] private BetController betController;

    // UI refs
    [Export] private Label actionLabel;
    [Export] private Label playerChipsLabel;
    [Export] private Label enemyChipsLabel;
    [Export] private Label potLabel;

    // test data
    [Export] public CardData testCardData;

    // runtime refs
    private RoundPhase currentPhase;
    private int currentPot = 0;
    private ChipManager playerChips;        // awarded to the winner after showdown

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        combatManager.Initialize();

        betController.OnBetPhaseEnd += EndBetPhase;
        betController.OnEnemyAction += DisplayEnemyAction;
        betController.OnChipsChanged += DisplayChips;

        playerChips = GetNode<ChipManager>("/root/GlobalManager/ChipManager");
        // TODO: temp implementation
        playerChips.AddChips(startingChips);

        StartCombatEncounter();
    }

    public void StartCombatEncounter()
    {
        StartPrePhase();
    }

    private void StartPrePhase()
    {
		if (!playerChips.Deduct(currentMinimumBuyIn)) return;   // Need a way to handle this
		enemyChips -= currentMinimumBuyIn;
        currentPhase = RoundPhase.PreRound;

        // draw starting hand for each lane
        combatManager.StartRound(startingDraw);
    }

    public void StartBetPhase()
    {
        currentPhase = RoundPhase.Betting;
        betController.BeginPhase(currentMinimumBuyIn, enemyChips);
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
            BetAction.AllIn => $"ALL-IN {called}",
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

    private void DisplayChips(string entity, int amount)
    {
        if (entity == "player" && playerChipsLabel != null)
            playerChipsLabel.Text = $"Chips: {amount}";

        else if (entity == "enemy" && enemyChipsLabel != null)
        {
            enemyChipsLabel.Text = $"Chips: {amount}";
            enemyChips = amount;
        }

        else if (entity == "pot" && potLabel != null)
        {
            potLabel.Text = $"Pot: {amount}";
            currentPot = amount;
        }
    }
}
