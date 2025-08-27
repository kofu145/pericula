using Godot;
using System;
using System.Collections.Generic;

public enum Turn { Player, Enemy, None }
public enum BetAction { None, Check, Raise, Call, Fold, CallAndRaise, AllIn }

public partial class BetController : Node
{
    // UI refs
    [Export] private TextureButton raiseButton;
    [Export] private TextureButton callButton;
    [Export] private TextureButton checkButton;
    [Export] private TextureButton foldButton;
    [Export] private TextureButton allInButton;

    [Export] private TextureButton raise1xButton;
    [Export] private TextureButton raise2xButton;
    [Export] private TextureButton raise5xButton;

    // Config
    private int minimumBuyIn;

    // public events
    public Action<BetAction, int, int> OnEnemyAction; // parameters: (action, toCallAmount, raiseAmount)
    public Action OnBetPhaseEnd;
    public Action<string, int> OnChipsChanged; // parameters: (entityName, newChipAmount)

    // names for event
    private string playerName = "player";
    private string enemyName = "enemy";
    private string potName = "pot";

    // states
    private Turn turn = Turn.None;
    private BetAction lastEnemyAction = BetAction.None;
    private BetAction lastPlayerAction = BetAction.None;

    // current bet
    private int currentBet = 0;     // highest amount put in by either side
    private bool betOpen = false;

    // each side's total bet this round
    private int playerPut = 0;
    private int enemyPut = 0;
    private int pot = 0;            // total for both sides

    private bool choosingRaiseAmount = false;

    private bool endedWithFold = false;
    private bool foldedByPlayer = false;

    private ChipManager playerChips;


    // temp implementation
    private RandomNumberGenerator rng;

    // ====================================
    private int PlayerBalance => playerChips.Balance;
    private int ToCall => Math.Max(0, currentBet - playerPut);
    private int AffordableRaise => Math.Max(0, PlayerBalance - ToCall);
    private bool CanCall => ToCall > 0 && PlayerBalance >= ToCall;
    private bool CanOpenRaise => !betOpen && PlayerBalance >= minimumBuyIn;
    private bool CanRaiseOverCall => betOpen && AffordableRaise >= minimumBuyIn;
    private bool ShouldShowAllIn => PlayerBalance > 0 && ToCall != PlayerBalance;
    // ====================================


    public override void _Ready()
    {
        playerChips = GetNode<ChipManager>("/root/GlobalManager/ChipManager");

        // wire button handlers
        if (raiseButton != null) raiseButton.Pressed += OnClickPlayerRaise;
        if (callButton != null) callButton.Pressed += OnClickPlayerCall;
        if (checkButton != null) checkButton.Pressed += OnClickPlayerCheck;
        if (foldButton != null) foldButton.Pressed += OnClickPlayerFold;
        if (allInButton != null) allInButton.Pressed += OnClickPlayerAllIn;

        HideAll();
        UpdateChipLabel();
    }
    public void BeginPhase(int minBuyIn)
    {
        // temp implementation of rng
        rng = new();
        rng.Randomize();

        minimumBuyIn = minBuyIn;

        if (raise1xButton != null)
        {
            raise1xButton.Pressed += () => OnClickPlayerRaiseAmount(1);
            raise1xButton.GetNode<Label>("Text").Text = $"$ {minimumBuyIn}";
        }
        if (raise2xButton != null)
        {
            raise2xButton.Pressed += () => OnClickPlayerRaiseAmount(2);
            raise2xButton.GetNode<Label>("Text").Text = $"$ {minimumBuyIn * 2}";
        }
        if (raise5xButton != null)
        {
            raise5xButton.Pressed += () => OnClickPlayerRaiseAmount(5);
            raise5xButton.GetNode<Label>("Text").Text = $"$ {minimumBuyIn * 5}";
        }

        // reset states for new bet
        turn = Turn.Player;
        lastEnemyAction = BetAction.None;
        lastPlayerAction = BetAction.None;

        playerPut = 0;
        enemyPut = 0;
        pot = 0;
        currentBet = 0;
        betOpen = false;

        endedWithFold = false;
        foldedByPlayer = false;

        choosingRaiseAmount = false;

        HideAll();
        Show(checkButton);
        Show(raiseButton);
        UpdateChipLabel();
    }


    // ============ Player Handlers =============
    private void OnClickPlayerRaise()
    {
        if (turn != Turn.Player) return;

        // guard: can't raise if not enough balance
        if ((!betOpen && PlayerBalance < minimumBuyIn) || (betOpen && AffordableRaise < minimumBuyIn))
            return;

        choosingRaiseAmount = true;
        UpdateButtons();
    }
    private void OnClickPlayerRaiseAmount(int multiplier)
    {
        if (turn != Turn.Player) return;

        int raiseAmount = minimumBuyIn * Math.Max(1, multiplier);
        int maxRaise = AffordableRaise;

        if (maxRaise <= 0)
        {
            OnClickPlayerAllIn();
            return;
        }

        raiseAmount = Math.Min(raiseAmount, maxRaise);  // clamp to max affordable
        ApplyPlayerRaise(raiseAmount);
    }

    private void ApplyPlayerRaise(int raiseAmount)
    {
        int toCall = ToCall;
        int balance = PlayerBalance;

        if (balance <= 0) return;
        if (balance < toCall)
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

        int maxRaise = Math.Max(0, balance - toCall);
        int finalRaise = Math.Min(Math.Max(0, raiseAmount), maxRaise);

        int spend = toCall + finalRaise;
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
        UpdateChipLabel();
        EnemyAct();
    }

    private void OnClickPlayerCall()
    {
        if (turn != Turn.Player) return;

        int toCall = ToCall;
        if (toCall <= 0) return;

        int spend = Math.Min(toCall, PlayerBalance);
        if (spend <= 0) return;

        // TODO: Apply 'toCall' to player singleton currency
        if (!playerChips.Deduct(spend)) return;

        playerPut += spend;
        pot += spend;

        lastPlayerAction = BetAction.Call;
        EndPhase();
    }

    private void OnClickPlayerCheck()
    {
        if (turn != Turn.Player) return;

        if (betOpen)
        {
            UpdateButtons();
            return;
        }

        lastPlayerAction = BetAction.Check;

        // both entities checked, end phase
        if (lastEnemyAction == BetAction.Check)
        {
            EndPhase();
            return;
        }

        turn = Turn.Enemy;
        UpdateButtons();
        UpdateChipLabel();
        EnemyAct();
    }

    private void OnClickPlayerFold()
    {
        if (turn != Turn.Player) return;

        lastPlayerAction = BetAction.Fold;
        endedWithFold = true;
        foldedByPlayer = true;
        EndPhase();
    }

    private void OnClickPlayerAllIn()
    {
        if (turn != Turn.Player) return;

        int balance = PlayerBalance;
        if (balance <= 0) return;

        // balance = Math.Min(balance, enemyBalance);   // TODO: Update with enemy balance

        pot += balance;
        currentBet = Math.Max(playerPut + balance, enemyPut);

        if (!playerChips.Deduct(balance)) return;

        lastPlayerAction = BetAction.AllIn;

        turn = Turn.Enemy;
        UpdateButtons();
        UpdateChipLabel();
        EnemyAct();
    }

    // ============ Enemy Logic (Temporary) =============
    private void EnemyAct()
    {
        if (turn != Turn.Enemy) return;
        GD.Print("Player last action: " + lastPlayerAction);

        if (lastPlayerAction == BetAction.AllIn)
        {
            // player went all in, enemy must Call or Fold
            // TODO: Replace with AI logic and not random logic
            var choice = rng.RandiRange(0, 1); // 0 = Check, 1 = Raise

            if (choice == 0)   // call
            {
                int toCall = Math.Max(0, currentBet - enemyPut);
                OnEnemyAction?.Invoke(BetAction.Call, toCall, 0);

                pot += toCall;
                enemyPut += toCall;
                // TODO: update enemy currency

            }
            else if (choice == 1)  // fold
            {
                lastEnemyAction = BetAction.Fold;
                OnEnemyAction?.Invoke(BetAction.Fold, 0, 0);

                endedWithFold = true;
                foldedByPlayer = false;
            }
            EndPhase();
            return;
        }

        else if (!betOpen)
        {
            // TODO: Replace with AI logic and not random logic
            // No open bet -> enemy randomly Check or Raise(open)
            var choice = rng.RandiRange(0, 1); // 0 = Check, 1 = Raise
            if (choice == 0)    // check
            {
                lastEnemyAction = BetAction.Check;
                OnEnemyAction?.Invoke(BetAction.Check, 0, 0);

                // Both checked -> end
                if (lastPlayerAction == BetAction.Check)
                {
                    EndPhase();
                    return;
                }

                turn = Turn.Player;
                UpdateButtons();
            }
            else    // check and raise
            {
                int amount = EnemyPickRaiseAmount();
                ApplyEnemyRaise(amount);
            }
            UpdateChipLabel();
            return;
        }


        // TODO: Replace with AI logic and not random logic
        // Bet is open -> enemy randomly Call / Raise / Fold
        var r = rng.RandiRange(0, 2); // 0=Call, 1=Raise, 2=Fold
        if (r == 0)
        {
            lastEnemyAction = BetAction.Call;

            int toCall = Math.Max(0, currentBet - enemyPut);
            OnEnemyAction?.Invoke(BetAction.Call, toCall, 0);
            // TODO: Apply 'toCall' to enemy currency

            enemyPut += toCall;
            pot += toCall;

            EndPhase(); // matched -> start battle
        }
        else if (r == 1)    // if raise
        {
            int amount = EnemyPickRaiseAmount();
            ApplyEnemyRaise(amount);
        }
        else    // if fold
        {
            lastEnemyAction = BetAction.Fold;
            OnEnemyAction?.Invoke(BetAction.Fold, 0, 0);

            endedWithFold = true;
            foldedByPlayer = false;
            EndPhase();
        }
    }

    // TODO: Replace random logic
    private int EnemyPickRaiseAmount()
    {
        int[] mults = { 1, 2, 5 };
        int m = mults[rng.RandiRange(0, mults.Length - 1)];
        return Math.Min(minimumBuyIn * m, PlayerBalance);
    }

    private void ApplyEnemyRaise(int amount)
    {
        int toCall = Math.Max(0, currentBet - enemyPut);
        int spend = toCall + amount;

        // TODO: apply changes to enemy currency
        currentBet += amount;
        enemyPut += spend;
        pot += spend;

        betOpen = currentBet > 0;
        lastEnemyAction = toCall > 0 ? BetAction.CallAndRaise : BetAction.Raise;
        OnEnemyAction?.Invoke(lastEnemyAction, toCall, amount);

        // Back to player to respond
        turn = Turn.Player;
        UpdateButtons();
        UpdateChipLabel();
    }

    // ============ UI & Helpers =============
    private void EndPhase()
    {
        turn = Turn.None;
        choosingRaiseAmount = false;
        HideAll();

        if (endedWithFold)
        {
            if (!foldedByPlayer) playerChips.AddChips(pot);
            // else  TODO: give pot to enemy
        }

        UpdateChipLabel();

        OnBetPhaseEnd?.Invoke();
        GD.Print("Phase is over");
    }

    private void UpdateButtons()
    {
        HideAll();


        switch (turn)
        {
            case Turn.Player:
                if (allInButton != null)
                {
                    var text = allInButton.GetNode<Label>("Text");
                    if (text != null) text.Text = $"All-In ($ {PlayerBalance})";
                    Show(allInButton);
                }
                if (choosingRaiseAmount)
                {
                    if (AffordableRaise >= minimumBuyIn) Show(raise1xButton);
                    if (AffordableRaise >= minimumBuyIn * 2) Show(raise2xButton);
                    if (AffordableRaise >= minimumBuyIn * 5) Show(raise5xButton);
                }
                else if (betOpen)
                {
                    if (CanCall) Show(callButton);
                    if (CanRaiseOverCall) Show(raiseButton);
                    Show(foldButton);
                }
                else
                {
                    Show(checkButton);
                    if (CanOpenRaise) Show(raiseButton);
                }
                break;

            case Turn.Enemy:

                break;
            case Turn.None:
                break;
        }
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

    private void Hide(TextureButton button)
    {
        if (button == null) return;
        button.Visible = false;
        button.Disabled = true;
    }

    private void Show(TextureButton button)
    {
        if (button == null) return;
        button.Visible = true;
        button.Disabled = false;
    }

    private void UpdateChipLabel()
    {
        OnChipsChanged?.Invoke(playerName, playerChips.Balance);
        OnChipsChanged?.Invoke(enemyName, -enemyPut); // TODO: Replace with enemy chips
        OnChipsChanged?.Invoke(potName, pot);
    }
}
