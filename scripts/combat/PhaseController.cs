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
	[Export] private int enemyChips = 100;

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
	private ChipManager playerChips;        // awarded to the winner after showdown
	private int currentTurn = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		combatManager.Initialize();

		betPhaseButton.Pressed += StartBetPhase;
		showdownButton.Pressed += StartShowdownPhase;

		betController.OnBetPhaseEnd += EndBetPhase;
		betController.OnEnemyAction += DisplayEnemyAction;
		betController.OnChipsChanged += DisplayChips;

		combatManager.OnShowdownEndPlayerWin += EndShowdownPhase;

		playerChips = GetNode<ChipManager>("/root/GlobalManager/ChipManager");
		// TODO: temp implementation
		playerChips.AddChips(startingChips);

		StartCombatEncounter();
	}

	public void StartCombatEncounter()
	{
		Show(betPhaseButton);
		Hide(showdownButton);

		StartPrePhase();
	}

	private void StartPrePhase()
	{
		if (currentTurn >= turnBuyInIncrease)
			currentMinimumBuyIn = (int)Math.Ceiling(currentMinimumBuyIn * buyInIncreaseMultiplier);


		turnLabel.Text = $"Turn: {currentTurn}";
		buyInLabel.Text = $"Current Buy In: {currentMinimumBuyIn}";


		if (!playerChips.Deduct(currentMinimumBuyIn)) return;   // Need a way to handle this for negative balance
		enemyChips -= currentMinimumBuyIn;
		currentPot += currentMinimumBuyIn * 2;

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
		if (endOnFold)
		{
			if (playerChips.Balance <= 0) OnEncounterOutcome?.Invoke(false);
			else if (enemyChips <= 0) OnEncounterOutcome?.Invoke(true);

			currentTurn++;
			StartCombatEncounter();
			return;
		}
		Show(showdownButton);
	}

	private void StartShowdownPhase()
	{
		currentPhase = RoundPhase.Showdown;
		combatManager.ShowdownHandler();
	}

	private void EndShowdownPhase(bool playerWon)
	{
		if (playerWon)
		{
			playerChips.AddChips(currentPot);
		}
		else
		{
			enemyChips += currentPot;
		}

		if (playerChips.Balance <= 0) OnEncounterOutcome?.Invoke(false);
		else if (enemyChips <= 0) OnEncounterOutcome?.Invoke(true);

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
