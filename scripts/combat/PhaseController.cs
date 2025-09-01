using System;
using Godot;

public enum RoundPhase { PreRound, Betting, Showdown }
public partial class PhaseController : Node
{
    [Export] private int startingDraw = 5;
    // buy in config
    [Export] private int turnBuyInIncrease = 4;
    [Export] private float buyInIncreaseMultiplier = 2;

    // Scene refs
    [Export] private CombatController combatManager;
    [Export] private BetController betController;

    // UI refs
    [Export] private Label enemyNameLabel;
    [Export] private Label playerChipsLabel;
    [Export] private Label negativeBalanceWarningLabel;
    [Export] private Label enemyChipsLabel;
    [Export] private Label potLabel;
    [Export] private Label turnLabel;
    [Export] private Label buyInLabel;
    [Export] private Label nextTurnBuyInLabel;

    [Export] private Control popupAnchor;

    // phase buttons
    [Export] private Button betPhaseButton;
    [Export] private Button showdownButton;
    [Export] private Button nextTurnButton;
    [Export] private Button endEncounterButton;

    // public actions
    /// <summary>
    /// true indicates player won encounter
    /// </summary>
    public Action<bool> OnEncounterOutcome;

    // runtime refs
    private RoundPhase currentPhase;
    private ChipManager playerChips;
    private EnemyChips enemyChips;
    private int currentTurn = 1;
    private int currentPot = 0;
    private int currentMinimumBuyIn = 0;
    private int playerContributionThisTurn;
    private int enemyContributionThisTurn;

    // =========================================
    private bool PlayerWon => enemyChips.Balance <= 0;
    private bool PlayerLost => playerChips.Balance <= 0;
    // =========================================

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        enemyNameLabel.Text = StageManager.Instance.GetCurrentEnemy().DisplayName;
        combatManager.Initialize();

        betPhaseButton.Pressed += StartBetPhase;
        showdownButton.Pressed += StartShowdownPhase;
        nextTurnButton.Pressed += StartNextTurn;
        endEncounterButton.Pressed += EndEncounter;

        betController.OnBetPhaseEnd += EndBetPhase;
        betController.OnEnemyAction += DisplayEnemyAction;
        betController.OnChipsChanged += UpdatePot;

        combatManager.OnShowdownEndPlayerWin += EndShowdownPhase;

        // add listeners to update enemy and player chips UI
        playerChips = ChipManager.Instance;
        playerChips.OnChipsChanged += DisplayPlayerChips;

        currentMinimumBuyIn = StageManager.Instance.GetCurrentBuyIn();

        enemyChips = new(StageManager.Instance.GetEnemyStartingChips());
        enemyChips.OnChipsChanged += DisplayEnemyChips;


        // force update when scene is first loaded
        UpdatePot(0);
        enemyChipsLabel.Text = enemyChips.Balance.ToString();
        playerChipsLabel.Text = playerChips.Balance.ToString();

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
        int buyIn = currentMinimumBuyIn;

        enemyContributionThisTurn = Math.Min(enemyChips.Balance, buyIn);
        if (enemyContributionThisTurn > 0) enemyChips.Deduct(enemyContributionThisTurn);

        if (playerChips.Balance >= buyIn)
        {
            playerChips.Deduct(buyIn);
            playerContributionThisTurn = buyIn;
        }
        else
        {
            playerChips.BorrowChips(buyIn);
            playerContributionThisTurn = buyIn;
        }

        currentPot = enemyContributionThisTurn + playerContributionThisTurn;

        UpdatePot(currentPot);
        turnLabel.Text = $"Turn: {currentTurn}";
        buyInLabel.Text = $"{currentMinimumBuyIn}";

        if (playerChips.Balance <= 0)
            negativeBalanceWarningLabel.Text = $"Warning. Losing next Showdown will lose you the run.";
        negativeBalanceWarningLabel.Visible = playerChips.Balance <= 0;

        currentPhase = RoundPhase.PreRound;

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
        SoundManager.PlaySE("click");
        Hide(betPhaseButton);
        currentPhase = RoundPhase.Betting;
        combatManager.StartBetPhase();

        betController.BeginPhase(
         currentMinimumBuyIn,
         enemyChips,
         currentPot,
         playerContributionThisTurn,
         enemyContributionThisTurn
     );
    }

    private void EndBetPhase(bool endOnFold)
    {
        if (endOnFold) Show(nextTurnButton);
        else Show(showdownButton);
    }

    private void StartShowdownPhase()
    {
        SoundManager.PlaySE("click");
        SoundManager.PlaySE("select_node");
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
            if (PlayerLost)
            {
                Show(endEncounterButton);
                endEncounterButton.Text = "Game Over";
            }
            else if (PlayerWon)
            {
                Show(endEncounterButton);
                if (StageManager.Instance.IsFinalEncounterOfRun) endEncounterButton.Text = "Continue";
            }
            else Show(nextTurnButton);   // optional maybe, different button that will start next turn AND claim reward
        }
        else
        {
            enemyChips.AddChips(currentPot);
            if (PlayerLost)
            {
                Show(endEncounterButton);
                endEncounterButton.Text = "Game Over";
            }
            else if (PlayerWon)
            {
                Show(endEncounterButton);
                if (StageManager.Instance.IsFinalEncounterOfRun) endEncounterButton.Text = "Continue";
            }
            else Show(nextTurnButton);
        }
        UpdatePot(0);
    }

    private void StartNextTurn()
    {
        SoundManager.PlaySE("click");
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
        UnbindEvents();
        RunEndManager.Instance.LoseRun();
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
        button.MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    private void Show(Button button)
    {
        if (button == null) return;
        button.Visible = true;
        button.Disabled = false;
        button.MouseFilter = Control.MouseFilterEnum.Stop;
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

        PopupText.Instance.ShowText(popupAnchor.Position, text, 5, .6f, true);
    }
    private void UpdatePot(int amount)
    {
        if (potLabel != null)
        {
            potLabel.Text = $"{amount}";
            currentPot = amount;
        }
    }
    private void DisplayEnemyChips(int amount)
    {
        int previousAmount = enemyChipsLabel.Text.ToInt();
        int difference = amount - previousAmount;
        if (difference > 0)
        {
            PopupText.Instance.ShowText(enemyChipsLabel.GlobalPosition, $"+{difference}");
        }
        else
        {
            PopupText.Instance.ShowText(enemyChipsLabel.GlobalPosition, $"{difference}");
        }

        SoundManager.PlaySE("chip_drop");
        enemyChipsLabel.Text = $"{amount}";
    }

    private void DisplayPlayerChips(int amount)
    {
        int previousAmount = playerChipsLabel.Text.ToInt();
        int difference = amount - previousAmount;
        if (difference > 0)
        {
            PopupText.Instance.ShowText(playerChipsLabel.GlobalPosition, $"+{difference}");
        }
        else
        {
            PopupText.Instance.ShowText(playerChipsLabel.GlobalPosition, $"{difference}");
        }

        SoundManager.PlaySE("chip_drop");
        playerChipsLabel.Text = $"{amount}";
    }


    private void UnbindEvents()
    {
        betController.OnBetPhaseEnd -= EndBetPhase;
        betController.OnEnemyAction -= DisplayEnemyAction;
        betController.OnChipsChanged -= UpdatePot;

        combatManager.OnShowdownEndPlayerWin -= EndShowdownPhase;

        enemyChips.OnChipsChanged = null;
        ChipManager.Instance.OnChipsChanged -= DisplayPlayerChips;
    }
}
