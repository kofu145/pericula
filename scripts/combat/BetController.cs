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
    [Export] private Label currentPotLabel;

    [Export] private TextureButton raise1xButton;
    [Export] private TextureButton raise2xButton;
    [Export] private TextureButton raise5xButton;

    // Config
    // minimum starting bet, will be increased by singleton instance as run progresses
    [Export] private int minimumBuyIn = 10;

    // public events
    public Action<BetAction, int, int> OnEnemyAction;
    // parameters: (action, toCallAmount, raiseAmount)
    public Action OnBetPhaseEnd;
    // parameters: (endedWithFold, foldedByPlayer)


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


    // temp implementation
    private RandomNumberGenerator rng;
    public override void _Ready()
    {
        if (raiseButton != null) raiseButton.Pressed += OnClickPlayerRaise;
        if (callButton != null) callButton.Pressed += OnClickPlayerCall;
        if (checkButton != null) checkButton.Pressed += OnClickPlayerCheck;
        if (foldButton != null) foldButton.Pressed += OnClickPlayerFold;

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

        HideAll();
        UpdatePotLabel();
    }
    public void BeginPhase()
    {
        // temp implementation of rng
        rng = new();
        rng.Randomize();

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
        UpdatePotLabel();
    }


    // ============ Player Handlers =============
    private void OnClickPlayerRaise()
    {
        if (turn != Turn.Player) return;

        choosingRaiseAmount = true;
        UpdateButtons();
    }
    private void OnClickPlayerRaiseAmount(int multiplier)
    {
        if (turn != Turn.Player) return;

        int raiseAmount = minimumBuyIn * Math.Max(1, multiplier);
        ApplyPlayerRaise(raiseAmount);
    }

    private void ApplyPlayerRaise(int raiseAmount)
    {
        int toCall = Math.Max(0, currentBet - playerPut);
        int spend = toCall + raiseAmount;

        // TODO: Apply 'spend' to player singleton currency
        playerPut += spend;
        pot += spend;

        currentBet = Math.Max(playerPut, enemyPut);
        betOpen = true;
        lastPlayerAction = BetAction.Raise;

        choosingRaiseAmount = false;
        turn = Turn.Enemy;

        UpdateButtons();
        UpdatePotLabel();
        EnemyAct();
    }

    private void OnClickPlayerCall()
    {
        if (turn != Turn.Player) return;

        int toCall = Math.Max(0, currentBet - playerPut);
        if (toCall <= 0) return;

        // TODO: Apply 'toCall' to player singleton currency

        playerPut += toCall;
        pot += toCall;

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
        UpdatePotLabel();
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

    // ============ Enemy Logic (Temporary) =============
    private void EnemyAct()
    {
        if (turn != Turn.Enemy) return;

        if (!betOpen)
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
            UpdatePotLabel();
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
        else
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
        return minimumBuyIn * m;
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
        UpdatePotLabel();
    }

    // ============ UI & Helpers =============
    private void EndPhase()
    {
        turn = Turn.None;
        choosingRaiseAmount = false;
        HideAll();
        UpdatePotLabel();
        OnBetPhaseEnd?.Invoke();
        GD.Print("Phase is over");
    }

    private void UpdateButtons()
    {
        HideAll();

        switch (turn)
        {
            case Turn.Player:
                if (choosingRaiseAmount)
                {
                    Show(raise1xButton);
                    Show(raise2xButton);
                    Show(raise5xButton);
                }
                else if (betOpen)
                {
                    Show(callButton);
                    Show(raiseButton);
                    Show(foldButton);
                }
                else
                {
                    Show(checkButton);
                    Show(raiseButton);
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

    private void UpdatePotLabel()
    {
        if (currentPotLabel == null) return;
        currentPotLabel.Text = $"Pot: {pot}";
    }
}
