using System;
using Godot;

public enum RoundPhase { PreRound, Betting, Showdown }
public partial class PhaseController : Node
{
	[Export] private int startingDraw = 5;
	[Export] private float displayDuration = 1.5f;

	// TODO: temp implementation
	[Export] private int startingChips = 100;
	// minimum starting bet, will be increased by singleton instance as run progresses
	// get info from the singleton instance
	[Export] private int currentMinimumBuyIn = 10;
	// get info from the enemy info(?) when the scene is initialized(?)
	[Export] private int enemyChipsAmount = 100;

	// buy in config
	[Export] private int turnBuyInIncrease = 4;
	[Export] private float buyInIncreaseMultiplier = 2;

	// Scene refs
	[Export] private CombatController combatManager;
	[Export] private BetController betController;

	// UI refs
	[Export] private Label actionLabel;
	[Export] private Label playerChipsLabel;
	[Export] private Label enemyChipsLabel;
	[Export] private Label potLabel;
	[Export] private Label turnLabel;
	[Export] private Label buyInLabel;

	// phase buttons
	[Export] private Button betPhaseButton;
	[Export] private Button showdownButton;
	[Export] private Button nextTurnButton;
	[Export] private Button endEncounterButton;

	// test data
	[Export] public CardData testCardData;

	// public actions
	/// <summary>
	/// true indicates player won encounter
	/// </summary>
	public Action<bool> OnEncounterOutcome;

	// runtime refs
	private RoundPhase currentPhase;
	private int currentPot = 0;
	private ChipManager playerChips;
	private EnemyChips enemyChips;
	private int currentTurn = 1;

	// =========================================
	private bool PlayerWon => enemyChips.Balance <= 0;
	private bool PlayerLost => playerChips.Balance <= 0;
	// =========================================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		combatManager.Initialize();

		betPhaseButton.Pressed += StartBetPhase;
		showdownButton.Pressed += StartShowdownPhase;
		nextTurnButton.Pressed += StartNextTurn;
		endEncounterButton.Pressed += EndEncounter;

		betController.OnBetPhaseEnd += EndBetPhase;
		betController.OnEnemyAction += DisplayEnemyAction;
		betController.OnChipsChanged += DisplayPot;

		combatManager.OnShowdownEndPlayerWin += EndShowdownPhase;

		playerChips = ChipManager.Instance;
		playerChips.OnChipsChanged += DisplayPlayerChips;

		enemyChips = new(enemyChipsAmount);
		enemyChips.OnChipsChanged += DisplayEnemyChips;

		// TODO: temp implementation
		playerChips.AddChips(startingChips);

		StartCombatEncounter();
	}

	public void StartCombatEncounter()
	{
		Show(betPhaseButton);
		Hide(showdownButton);
		Hide(nextTurnButton);
		Hide(endEncounterButton);

		StartPrePhase();
	}

	private void StartPrePhase()
	{
		if (currentTurn >= turnBuyInIncrease)
			currentMinimumBuyIn = (int)Math.Ceiling(currentMinimumBuyIn * buyInIncreaseMultiplier);


		turnLabel.Text = $"Turn: {currentTurn}";
		buyInLabel.Text = $"Current Buy In: {currentMinimumBuyIn}";


		if (!playerChips.Deduct(currentMinimumBuyIn)) return;   // Need a way to handle this for negative balance
		if (!enemyChips.Deduct(currentMinimumBuyIn)) return;

		currentPot = currentMinimumBuyIn * 2;

		currentPhase = RoundPhase.PreRound;

		// draw starting hand for each lane
		combatManager.StartRound(startingDraw);
	}

	public void StartBetPhase()
	{
		Hide(betPhaseButton);
		currentPhase = RoundPhase.Betting;
		betController.BeginPhase(currentMinimumBuyIn, enemyChips, currentPot);
	}

	private void EndBetPhase(bool endOnFold)
	{
		if (endOnFold) Show(nextTurnButton);
		else Show(showdownButton);
	}

	private void StartShowdownPhase()
	{
		Hide(showdownButton);
		currentPhase = RoundPhase.Showdown;
		combatManager.ShowdownHandler();
	}

	private void EndShowdownPhase(bool playerWonCombat)
	{
		// should check which button to show, start next turn or go to shop
		combatManager.EndCombat();
		if (playerWonCombat)
		{
			playerChips.AddChips(currentPot);
			if (PlayerLost || PlayerWon) Show(endEncounterButton);
			else Show(nextTurnButton);   // optional maybe, different button that will start next turn AND claim reward
		}
		else
		{
			enemyChips.AddChips(currentPot);
			if (PlayerLost || PlayerWon) Show(endEncounterButton);
			else Show(nextTurnButton);
		}
	}

	private void StartNextTurn()
	{
		combatManager.EndTurn();
		DisplayPot(0);
		currentTurn++;
		StartCombatEncounter();
	}

	private void EndEncounter()
	{
		if (PlayerLost)
		{
			// player lost
			OnEncounterOutcome?.Invoke(false);
			EndCurrentRun();
		}

		if (PlayerWon)
		{
			// player won
			OnEncounterOutcome?.Invoke(true);
			DisplaySummary();
		}
	}

	private void EndCurrentRun()
	{
		GD.Print("Game over! Lost on stage " + StageManager.Instance.CurrentStageID);
	}

	private void DisplaySummary()
	{
		StageManager.Instance.CompleteStage();
	}

	private void Hide(Button button)
	{
		if (button == null) return;
		button.Visible = false;
		button.Disabled = true;
	}

	private void Show(Button button)
	{
		if (button == null) return;
		button.Visible = true;
		button.Disabled = false;
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

	private void DisplayPot(int amount)
	{
		if (potLabel != null)
		{
			potLabel.Text = $"Pot: {amount}";
			currentPot = amount;
		}
	}
	private void DisplayEnemyChips(int amount)
	{
		enemyChipsLabel.Text = $"Chips: {amount}";
	}

	private void DisplayPlayerChips(int amount)
	{
		playerChipsLabel.Text = $"Chips: {amount}";
	}
}
