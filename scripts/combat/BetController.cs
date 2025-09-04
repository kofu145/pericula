
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public enum Turn { Player, Enemy, None }
public enum BetAction { None, Check, Raise, Call, Fold, CallAndRaise, AllIn }

public partial class BetController : Node
{
    // UI refs
    [Export] private Button raiseButton;
    [Export] private Button callButton;
    [Export] private Button checkButton;
    [Export] private Button foldButton;
    [Export] private Button allInButton;

    [Export] private Button raise1xButton;
    [Export] private Button raise2xButton;
    [Export] private Button raise5xButton;

    [Export] private float opponentDelayAfterPlayerBet = 0.5f;
    [Export] public CombatController combatManager;

    private int minimumBuyIn;

    // public events
    public Action<BetAction, int, int> OnEnemyAction;
    public Action<bool> OnBetPhaseEnd;
    public Action<int> OnChipsChanged;

    // states
    private Turn turn = Turn.None;
    private BetAction lastEnemyAction = BetAction.None;
    private BetAction lastPlayerAction = BetAction.None;

    private int currentBet = 0;
    private bool betOpen = false;

    private int playerPut = 0;
    private int enemyPut = 0;
    private int pot = 0;

    private EnemyChips enemyChips;
    private bool choosingRaiseAmount = false;
    private bool endedWithFold = false;
    private bool foldedByPlayer = false;
    private ChipManager playerChips;
    private int playerHandScore = 0;
    private int enemyHandScore = 0;

    private bool hasOnlyAnte = false; // NEW: flag to distinguish ante from open bet

    // ==========================
    private int PlayerBalance => playerChips.Balance;
    private int EnemyBalance => enemyChips.Balance;
    private int ToCall => hasOnlyAnte ? 0 : Math.Max(0, Math.Min(currentBet - playerPut, PlayerBalance));
    private int AffordableRaise => Math.Max(0, PlayerBalance - ToCall);
    private bool CanCall => ToCall > 0;
    private bool CanOpenRaise => !betOpen && PlayerBalance >= minimumBuyIn && EnemyBalance >= minimumBuyIn;
    private bool CanRaiseOverCall => betOpen && AffordableRaise >= minimumBuyIn;
    private bool CanBet => PlayerBalance >= minimumBuyIn;
    // ==========================

    public override void _Ready()
    {
        playerChips = ChipManager.Instance;

        // wire buttons
        if (raiseButton != null) raiseButton.Pressed += OnClickPlayerRaise;
        if (callButton != null) callButton.Pressed += OnClickPlayerCall;
        if (checkButton != null) checkButton.Pressed += OnClickPlayerCheck;
        if (foldButton != null) foldButton.Pressed += OnClickPlayerFold;
        if (allInButton != null) allInButton.Pressed += OnClickPlayerAllIn;
        if (raise1xButton != null) raise1xButton.Pressed += () => OnClickPlayerRaiseAmount(1);
        if (raise2xButton != null) raise2xButton.Pressed += () => OnClickPlayerRaiseAmount(2);
        if (raise5xButton != null) raise5xButton.Pressed += () => OnClickPlayerRaiseAmount(5);

        HideAll();
        UpdatePotLabel();
    }

    public void BeginPhase(int minBuyIn, EnemyChips enemyChips,
        int startingPot, int playerContributes, int enemyContributes)
    {
        minimumBuyIn = minBuyIn;
        this.enemyChips = enemyChips;
        pot = startingPot;
        playerPut = playerContributes;
        enemyPut = enemyContributes;


        currentBet = Math.Max(playerPut, enemyPut);
        betOpen = false; // ante does not count as an open bet
        hasOnlyAnte = (playerPut > 0 || enemyPut > 0);

        turn = Turn.Player;
        lastEnemyAction = BetAction.None;
        lastPlayerAction = BetAction.None;
        endedWithFold = false;
        foldedByPlayer = false;
        choosingRaiseAmount = false;

        CalcScoreAndSetOdds();

        if (raise1xButton != null) raise1xButton.Text = $"$ {minimumBuyIn}";
        if (raise2xButton != null) raise2xButton.Text = $"$ {minimumBuyIn * 2}";
        if (raise5xButton != null) raise5xButton.Text = $"$ {minimumBuyIn * 5}";

        HideAll();
        UpdatePotLabel();
        UpdateButtons();
    }

    // ============ Player Handlers =============
    private void OnClickPlayerRaise()
    {
        if (turn != Turn.Player) return;

        // guard: can't raise if not enough balance
        if ((!betOpen && PlayerBalance < minimumBuyIn) || (betOpen && AffordableRaise < minimumBuyIn))
            return;

        SoundManager.PlaySE("click");
        choosingRaiseAmount = true;
        UpdateButtons();
    }
    private void OnClickPlayerRaiseAmount(int multiplier)
    {
        if (turn != Turn.Player) return;

        int raiseAmount = minimumBuyIn * Math.Max(1, multiplier);
        int maxRaise = AffordableRaise;     // maxRaise capped to enemy balance, if enemy is the one who raised, it should cap to enemyBalance - ToCall

        if (maxRaise <= 0)
        {
            OnClickPlayerAllIn();
            return;
        }

        raiseAmount = Math.Min(raiseAmount, maxRaise + ToCall);  // clamp to max affordable
        SoundManager.PlaySE("click");
        ApplyPlayerRaise(raiseAmount);
    }

    private void ApplyPlayerRaise(int raiseAmount)
    {
        int amountToCall = ToCall;
        int balance = PlayerBalance;

        if (balance <= 0) return;
        if (balance < amountToCall)
        {
            // all in remaining
            int spendAll = balance;
            if (!playerChips.Deduct(spendAll)) return;
            playerPut += spendAll;
            pot += spendAll;
            lastPlayerAction = BetAction.AllIn; // all in to call
            EndPhase();
            return;
        }

        int maxRaise = Math.Max(0, balance - amountToCall);
        int finalRaise = Math.Min(Math.Max(0, raiseAmount), maxRaise);

        int spend = amountToCall + finalRaise;
        if (spend <= 0) return;

        if (!playerChips.Deduct(spend)) return;
        playerPut += spend;
        pot += spend;

        currentBet = Math.Max(playerPut, enemyPut);
        betOpen = true;
        lastPlayerAction = finalRaise > 0 ? BetAction.Raise : BetAction.Call;

        choosingRaiseAmount = false;
        turn = Turn.Enemy;

        UpdateButtons();
        UpdatePotLabel();
        EnemyAct();
    }

    private void OnClickPlayerCall()
    {
        if (turn != Turn.Player) return;

        int amountToCall = ToCall;
        if (amountToCall <= 0) return;

        int spend = Math.Min(amountToCall, PlayerBalance);
        if (spend <= 0) return;

        if (!playerChips.Deduct(spend)) return;

        playerPut += spend;
        pot += spend;

        SoundManager.PlaySE("click");
        lastPlayerAction = BetAction.Call;
        EndPhase();
    }

    private void OnClickPlayerCheck()
    {
        if (turn != Turn.Player) return;

        if (betOpen)
        {
            // UpdateButtons();
            return;
        }

        lastPlayerAction = BetAction.Check;

        // both entities checked, end phase
        if (lastEnemyAction == BetAction.Check)
        {
            EndPhase();
            return;
        }

        SoundManager.PlaySE("click");
        turn = Turn.Enemy;
        UpdateButtons();
        UpdatePotLabel();
        EnemyAct();
    }

    private void OnClickPlayerFold()
    {
        if (turn != Turn.Player) return;

        lastPlayerAction = BetAction.Fold;
        endedWithFold = true;
        foldedByPlayer = true;
        SoundManager.PlaySE("click");
        EndPhase();
    }

    private void OnClickPlayerAllIn()
    {
        if (turn != Turn.Player) return;

        int balance = PlayerBalance;
        if (balance <= 0) return;

        balance = Math.Min(balance, EnemyBalance + ToCall);

        if (!playerChips.Deduct(balance)) return;
        pot += balance;
        playerPut += balance;
        currentBet = Math.Max(playerPut, enemyPut);


        lastPlayerAction = BetAction.AllIn;

        turn = Turn.Enemy;
        SoundManager.PlaySE("click");
        UpdateButtons();
        UpdatePotLabel();
        EnemyAct();
    }

    // ============ Enemy Logic (Temporary) =============
    private async Task EnemyAct()
    {
        if (turn != Turn.Enemy) return;
        await WaitFor(opponentDelayAfterPlayerBet);
        GD.Print("Player last action: " + lastPlayerAction);

        int amountToCall = Math.Max(0, currentBet - enemyPut);

        // -------------------------
        // CASE 1: Player went all-in
        // -------------------------
        if (lastPlayerAction == BetAction.AllIn)
        {
            int spend = Math.Min(amountToCall, EnemyBalance);
            if (spend >= EnemyBalance)
            {
                spend = EnemyBalance;
                lastEnemyAction = BetAction.AllIn;
                OnEnemyAction?.Invoke(BetAction.AllIn, spend, 0);
            }
            else
            {
                lastEnemyAction = BetAction.Call;
                OnEnemyAction?.Invoke(BetAction.Call, spend, 0);
            }

            if (!enemyChips.Deduct(spend))
            {
                GD.PushError("Enemy does not have enough chips for action.");
                return;
            }

            enemyPut += spend;
            pot += spend;

            EndPhase();
            return;
        }

        // -------------------------
        // CASE 2: No open bet, nothing to call → enemy can only check
        // -------------------------
        if (!betOpen && amountToCall == 0)
        {
            lastEnemyAction = BetAction.Check;
            OnEnemyAction?.Invoke(BetAction.Check, 0, 0);

            // If both checked, end phase
            if (lastPlayerAction == BetAction.Check)
            {
                EndPhase();
                return;
            }

            // back to player
            turn = Turn.Player;
            UpdateButtons();
            return;
        }

        // -------------------------
        // CASE 3: Normal betting logic
        // -------------------------
        int choice = GetWeightedAction(); // 0 = call, 1 = fold, 2 = raise
        if (choice == 0)
        {
            var doCall = PercentChance(50);
            choice = doCall ? 0 : 2;
        }
        if (!CanBet) choice = GetWeightedAction();          // cannot raise? force call or fold

        switch (choice)
        {
            case 0: // Call
                int callAmount = Math.Min(amountToCall, EnemyBalance);
                lastEnemyAction = callAmount >= EnemyBalance ? BetAction.AllIn : BetAction.Call;
                OnEnemyAction?.Invoke(lastEnemyAction, callAmount, 0);

                if (!enemyChips.Deduct(callAmount))
                {
                    GD.PushError("Enemy does not have enough chips for action.");
                    return;
                }

                enemyPut += callAmount;
                pot += callAmount;

                EndPhase();
                break;

            case 1: // Fold
                lastEnemyAction = BetAction.Fold;
                OnEnemyAction?.Invoke(BetAction.Fold, 0, 0);
                endedWithFold = true;
                foldedByPlayer = false;
                EndPhase();
                break;

            case 2: // Raise
                int raiseAmount = EnemyPickRaiseAmount();
                ApplyEnemyRaise(raiseAmount);
                break;
        }
    }

    // TODO: Replace random logic
    private int EnemyPickRaiseAmount()
    {
        int[] mults = { 1, 2, 5 };
        int m = mults[DeckManager.Instance.RndGen.Next(mults.Length)];
        // cap the value to the lower of the enemyBalance or playerBalance and clamp to 0.
        int betCap = Math.Max(0, Math.Min(EnemyBalance, PlayerBalance));
        return Math.Min(minimumBuyIn * m, betCap);
    }

    private void ApplyEnemyRaise(int amount)
    {
        int amountToCall = Math.Max(0, currentBet - enemyPut);
        int spend = amountToCall + amount;

        if (spend >= EnemyBalance)
        {
            // all in
            spend = EnemyBalance;
            lastEnemyAction = BetAction.AllIn;
            OnEnemyAction?.Invoke(lastEnemyAction, spend, 0);
        }
        else
        {
            lastEnemyAction = amountToCall > 0 ? BetAction.CallAndRaise : BetAction.Raise;
            OnEnemyAction?.Invoke(lastEnemyAction, amountToCall, amount);
        }

        if (!enemyChips.Deduct(spend))
        {
            GD.PushError("Enemy does not have enough chips for action.");
            return;
        }

        currentBet += Math.Max(0, spend - amountToCall);
        enemyPut += spend;
        pot += spend;

        betOpen = currentBet > 0;

        // Back to player to respond
        if (lastEnemyAction == BetAction.AllIn && enemyPut == playerPut)
        {
            EndPhase();
            return;
        }

        turn = Turn.Player;
        UpdateButtons();
        UpdatePotLabel();
    }

    // ============ UI & Helpers =============
    private void EndPhase()
    {
        turn = Turn.None;
        choosingRaiseAmount = false;
        HideAll();

        if (endedWithFold)
        {
            if (!foldedByPlayer)    // player win
            {
                RunManager.Instance.PlayerWonBet();
                int difference = Math.Max(0, enemyPut - playerPut);
                playerChips.AddChips(pot - difference);
                enemyChips.AddChips(difference);
            }
            else    // player loss
            {
                RunManager.Instance.PlayerLostBet();
                int difference = Math.Max(0, playerPut - enemyPut);
                enemyChips.AddChips(pot - difference);
                playerChips.AddChips(difference);
            }
        }

        UpdatePotLabel();

        OnBetPhaseEnd?.Invoke(endedWithFold);
        GD.Print("Phase is over");
    }

    private void UpdateButtons()
    {
        HideAll();
        if (turn == Turn.Player)
        {
            if (allInButton != null)
            {
                allInButton.Text = $"All-In ($ {Math.Max(0, Math.Min(PlayerBalance, EnemyBalance + ToCall))})";
                if (CanBet) Show(allInButton);
            }

            if (choosingRaiseAmount)
            {
                // Show only raise multiplier buttons
                if (AffordableRaise >= minimumBuyIn && EnemyBalance >= minimumBuyIn) Show(raise1xButton);
                if (AffordableRaise >= minimumBuyIn * 2 && EnemyBalance >= minimumBuyIn * 2) Show(raise2xButton);
                if (AffordableRaise >= minimumBuyIn * 5 && EnemyBalance >= minimumBuyIn * 5) Show(raise5xButton);

                // Hide main raise and fold buttons while choosing amount
                Hide(raiseButton);
                Hide(foldButton);
                Hide(checkButton);
                Hide(callButton);
            }
            else if (betOpen)
            {
                // Player facing an open bet
                if (CanCall && ToCall > 0)
                {
                    Show(callButton);
                    callButton.Text = $"Call ($ {ToCall})";
                }

                if (CanRaiseOverCall)
                {
                    Show(raiseButton);
                }

                // Show fold if there is a bet to respond to
                if (CanBet) Show(foldButton);

                // Hide check button when there's an open bet
                Hide(checkButton);
            }
            else
            {
                // No bet open, player can check or open raise
                Show(checkButton);

                if (CanOpenRaise)
                {
                    Show(raiseButton);
                }

                // Hide call/fold buttons when nothing to call
                Hide(callButton);
                Hide(foldButton);
            }
        }
    }
    // chance to call 
    // 0 = call, 1 = fold - flipped false 
    // 0 = check, 1= raise - flipped true 
    private int GetWeightedAction()
    {
        var toCallIfGoodHand = 95;
        var toCallifBadHand = 50;
        int chance = enemyHandScore > playerHandScore ? toCallIfGoodHand : toCallifBadHand;
        // If enemy hand stronger, 95% chance to return call
        // if enemy hand weaker/equal, 40% chance to call, 60% chance to fold 
        bool hitChance = PercentChance(chance);

        //   - When facing all-in or open bet:   0 = Call, 1 = Fold
        //   - When no bet is open:              0 = Raise, 1 = Check
        return hitChance ? 0 : 1;
    }

    private void CalcScoreAndSetOdds()
    {
        // we evaluate the full hand of ourselves for score, but only 3 of theirs
        // to make up the difference we add an average amount to estimate what it might be
        int playerBonus = 8;
        playerHandScore = combatManager.CalculateHandScore(true) + playerBonus;
        enemyHandScore = combatManager.CalculateHandScore(false, 5);
        GD.Print($"is enemy hand better? {enemyHandScore > playerHandScore}");

    }

    private bool PercentChance(int percentage)
    {
        return DeckManager.Instance.RndGen.Next(100) < percentage;
    }

    private void HideAll()
    {
        Hide(raiseButton);
        Hide(callButton);
        Hide(checkButton);
        Hide(foldButton);
        Hide(allInButton);
        Hide(raise1xButton);
        Hide(raise2xButton);
        Hide(raise5xButton);
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

    private void UpdatePotLabel()
    {
        OnChipsChanged?.Invoke(pot);
    }

    // delay
    private async Task WaitFor(float seconds)
    {
        var timer = GetTree().CreateTimer(seconds);
        await ToSignal(timer, "timeout");
    }
}
