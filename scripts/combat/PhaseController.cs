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
	private EnemyChips enemyChips;
	private int currentTurn = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		combatManager.Initialize();

		betPhaseButton.Pressed += StartBetPhase;
		showdownButton.Pressed += StartShowdownPhase;
		nextTurnButton.Pressed += StartNextTurn;

		betController.OnBetPhaseEnd += EndBetPhase;
		betController.OnEnemyAction += DisplayEnemyAction;
		betController.OnChipsChanged += DisplayPot;

		combatManager.OnShowdownEndPlayerWin += EndShowdownPhase;

		ChipManager.Instance.OnChipsChanged += DisplayPlayerChips;

		enemyChips = new(enemyChipsAmount);
		enemyChips.OnChipsChanged += DisplayEnemyChips;

		// TODO: temp implementation
		ChipManager.Instance.AddChips(startingChips);

		StartCombatEncounter();
	}

	public void StartCombatEncounter()
	{
		Show(betPhaseButton);
		Hide(showdownButton);
		Hide(nextTurnButton);

		StartPrePhase();
	}

	private void StartPrePhase()
	{
		if (currentTurn >= turnBuyInIncrease)
			currentMinimumBuyIn = (int)Math.Ceiling(currentMinimumBuyIn * buyInIncreaseMultiplier);


		turnLabel.Text = $"Turn: {currentTurn}";
		buyInLabel.Text = $"Current Buy In: {currentMinimumBuyIn}";


		if (!ChipManager.Instance.Deduct(currentMinimumBuyIn)) return;   // Need a way to handle this for negative balance
		if (!enemyChips.Deduct(currentMinimumBuyIn)) return;

		currentPot = currentMinimumBuyIn * 2;

		currentPhase = RoundPhase.PreRound;

		// draw starting hand for each lane
		combatManager.StartRound(startingDraw);
	}

	public void StartBetPhase()
	{
		currentPhase = RoundPhase.Betting;
		betController.BeginPhase(currentMinimumBuyIn, enemyChips, currentPot);
		Hide(betPhaseButton);
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

	private void EndShowdownPhase(bool playerWon)
	{
		if (playerWon)
		{
			ChipManager.Instance.AddChips(currentPot);
			Show(nextTurnButton);   // optional maybe, different button that will start next turn AND claim reward
		}
		else
		{
			enemyChips.AddChips(currentPot);
			Show(nextTurnButton);
		}
	}

	private void StartNextTurn()
	{
		if (ChipManager.Instance.Balance <= 0)
		{
			OnEncounterOutcome?.Invoke(false);
			combatManager.EndCombat();
			GD.Print("Game over! Lost on stage " + StageManager.Instance.CurrentStageID);
			return;
		}
		else if (enemyChips.Balance <= 0)
		{
			OnEncounterOutcome?.Invoke(true);
			combatManager.EndCombat();
			StageManager.Instance.CompleteStage();
			return;
		}

		combatManager.EndTurn();
		currentTurn++;
		StartCombatEncounter();
		return;
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

	protected override void Dispose(bool disposing)
    {
        ChipManager.Instance.OnChipsChanged = null;
        base.Dispose(disposing);
    }
}
