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
    [Export] private Label negativeBalanceWarningLabel;
    [Export] private Label enemyChipsLabel;
    [Export] private Label potLabel;
    [Export] private Label turnLabel;
    [Export] private Label buyInLabel;
    [Export] private Label nextTurnBuyInLabel;

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

        // add listeners to update enemy and player chips UI
        playerChips = ChipManager.Instance;
        playerChips.OnChipsChanged += DisplayPlayerChips;

        enemyChips = new(enemyChipsAmount);
        enemyChips.OnChipsChanged += DisplayEnemyChips;

        // TODO: temp implementation
        playerChips.AddChips(startingChips);

        // force update when scene is first loaded
        DisplayPot(0);
        DisplayPlayerChips(playerChips.Balance);
        DisplayEnemyChips(enemyChips.Balance);


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
        // increase the buy in
        if (currentTurn >= turnBuyInIncrease)
            currentMinimumBuyIn = (int)Math.Ceiling(currentMinimumBuyIn * buyInIncreaseMultiplier);

        // display next turn's buy in
        nextTurnBuyInLabel.Visible = currentTurn >= turnBuyInIncrease - 1;
        nextTurnBuyInLabel.Text = $"Next Turn Buy In: {(int)Math.Ceiling(currentMinimumBuyIn * buyInIncreaseMultiplier)}";

        // shw the turn count
        turnLabel.Text = $"Turn: {currentTurn}";
        buyInLabel.Text = $"Current Buy In: {currentMinimumBuyIn}";

        // enemy and player pays the buyIn amount

        if (!enemyChips.Deduct(currentMinimumBuyIn)) currentMinimumBuyIn = enemyChips.Balance;

        enemyChips.Deduct(currentMinimumBuyIn);
        // if (!playerChips.Deduct(currentMinimumBuyIn)) return;

        // TODO: Need a way to handle this for negative balance
        playerChips.BorrowChips(currentMinimumBuyIn);
        if (playerChips.Balance <= 0) negativeBalanceWarningLabel.Text = $"Warning. Losing next Showdown will lose you the run.";
        negativeBalanceWarningLabel.Visible = playerChips.Balance <= 0;

        DisplayPot(currentMinimumBuyIn * 2);

        currentPhase = RoundPhase.PreRound;

        // draw starting hand for each lane
        combatManager.StartRound(startingDraw);
        if (enemyChips.Balance == 0 || playerChips.Balance <= 0)
        {
            // skip the bet phase because either side cannot bet
            Hide(betPhaseButton);
            Show(showdownButton);
        }
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
        DisplayPot(0);
    }

    private void StartNextTurn()
    {
        combatManager.EndTurn();
        currentTurn++;
        StartCombatEncounter();
    }

    private void EndEncounter()
    {
        combatManager.EndCombat();
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

    // loss handler
    private void EndCurrentRun()
    {
        // TODO: Wire back to the title screen
    }

    // win handler
    private void DisplaySummary()
    {
        UnbindEvents();
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

    protected override void Dispose(bool disposing)
    {
        // betController.OnBetPhaseEnd -= EndBetPhase;
        // betController.OnEnemyAction -= DisplayEnemyAction;
        // betController.OnChipsChanged -= DisplayPot;

        // combatManager.OnShowdownEndPlayerWin -= EndShowdownPhase;

        // enemyChips.OnChipsChanged = null;
        // ChipManager.Instance.OnChipsChanged = null;
        // base.Dispose(disposing);
    }

    private void UnbindEvents()
    {
        betController.OnBetPhaseEnd -= EndBetPhase;
        betController.OnEnemyAction -= DisplayEnemyAction;
        betController.OnChipsChanged -= DisplayPot;

        combatManager.OnShowdownEndPlayerWin -= EndShowdownPhase;

        enemyChips.OnChipsChanged = null;
		ChipManager.Instance.OnChipsChanged -= DisplayPlayerChips;
    }
}
