using Godot;
using System;

public enum Turn { Player, Enemy, None }
public enum BetAction { None, Check, Raise, call, Fold }

public partial class BetController : Node
{
    // UI refs
    [Export] private TextureButton raiseButton;
    [Export] private TextureButton callButton;
    [Export] private TextureButton checkButton;
    [Export] private TextureButton foldButton;
    [Export] private Label currentPotLabel;
    [Export] private SpinBox amountSpinBox;

    // Config
    [Export] private int openBetBase = 10;
    [Export] private float raisePercentStep = 0.1f;

    // public events
    public Action OnBetPhaseEnd;

    // states
    private Turn turn = Turn.None;
    private BetAction lastEnemyAction = BetAction.None;
    private BetAction lastPlayerAction = BetAction.None;

    private int currentBet = 0;
    private bool betOpen = false;
    private bool EndedWithFold = false;
    private bool FoldedByPlayer = false;


    // temp implementation
    private RandomNumberGenerator rng;



    public void BeginPhase()
    {
        // temp implementation of rng
        rng = new();
        rng.Randomize();

        turn = Turn.Player;
        lastEnemyAction = BetAction.None;
        lastPlayerAction = BetAction.None;
        betOpen = false;
        EndedWithFold = false;
        FoldedByPlayer = false;

        raiseButton.Pressed += OnClickPlayerRaise;
        callButton.Pressed += OnClickPlayerCall;
        checkButton.Pressed += OnClickPlayerCheck;
        foldButton.Pressed += OnClickPlayerFold;

        HideAll();
        Show(checkButton);
        Show(raiseButton);
    }


    // ============ Player Handlers =============
    private void OnClickPlayerRaise()
    {
        if (turn != Turn.Player) return;

        int amount = GetQuantizedRaise();
        if (amount <= 0)
        {
            ConfigureRaiseSpinBox();
            UpdateButtons();
            return;
        }

        currentBet += amount;
        betOpen = currentBet > 0;
        lastPlayerAction = BetAction.Raise;

        turn = Turn.Enemy;
        UpdateButtons();
        UpdatePotLabel();
        EnemyAct();
    }

    private void OnClickPlayerCall()
    {
        if (turn != Turn.Player) return;

        lastPlayerAction = BetAction.call;
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
        EndedWithFold = true;
        FoldedByPlayer = true;
        EndPhase();
    }

    // ============ Enemy Logic (Temporary) =============
    // TODO: Replace with AI logic and not random logic
    private void EnemyAct()
    {
        if (turn != Turn.Enemy) return;

        if (!betOpen)
        {
            // No open bet -> enemy randomly Check or Raise(open)
            var choice = rng.RandiRange(0, 1); // 0 = Check, 1 = Raise
            if (choice == 0)
            {
                lastEnemyAction = BetAction.Check;

                // Both checked -> end
                if (lastPlayerAction == BetAction.Check)
                {
                    EndPhase();
                    return;
                }

                turn = Turn.Player;
                UpdateButtons();
                ConfigureRaiseSpinBox();
            }
            else
            {
                int amount = GetQuantizedRaise();
                currentBet += amount;
                betOpen = currentBet > 0;

                lastEnemyAction = BetAction.Raise;
                turn = Turn.Player; // must Call/Raise/Fold
                UpdateButtons();
                ConfigureRaiseSpinBox();
            }
            UpdatePotLabel();
            return;
        }

        // Bet is open -> enemy randomly Call / Raise / Fold
        var r = rng.RandiRange(0, 2); // 0=Call, 1=Raise, 2=Fold
        if (r == 0)
        {
            lastEnemyAction = BetAction.call;
            EndPhase(); // matched -> start battle
        }
        else if (r == 1)
        {
            int amount = GetQuantizedRaise();
            currentBet += amount;
            betOpen = true;

            lastEnemyAction = BetAction.Raise;
            turn = Turn.Player; // respond
            UpdateButtons();
            ConfigureRaiseSpinBox();
        }
        else
        {
            lastEnemyAction = BetAction.Fold;
            EndedWithFold = true;
            FoldedByPlayer = false;
            EndPhase();
        }
    }


    // ============ UI & Helpers =============
    private void EndPhase()
    {
        turn = Turn.None;
        HideAll();
        UpdatePotLabel();
        OnBetPhaseEnd?.Invoke();
    }

    private void UpdateButtons()
    {
        HideAll();

        switch (turn)
        {
            case Turn.Player:
                if (betOpen)
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
                if (amountSpinBox != null) amountSpinBox.Visible = raiseButton.Visible;
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
        amountSpinBox.Visible = false;
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
        currentPotLabel.Text = $"Pot: {currentBet}";
    }

    private int GetQuantizedRaise()
    {
        if (amountSpinBox == null) return 0;

        int baseValue = (currentBet == 0) ? currentBet : openBetBase;

        int step = Math.Max(1, Mathf.CeilToInt(baseValue * Mathf.Clamp(raisePercentStep, 0.1f, 1f)));

        int raw = Mathf.Max(baseValue, Mathf.RoundToInt((float)amountSpinBox.Value));
        int k = Mathf.CeilToInt(raw / (float)step);
        int quant = step * k;

        amountSpinBox.Value = quant;
        return quant;
    }

    private void ConfigureRaiseSpinBox()
    {
        if (amountSpinBox == null) return;

        int baseValue = (currentBet > 0) ? currentBet : openBetBase;

        int step = Math.Max(1, Mathf.CeilToInt(baseValue * Mathf.Clamp(raisePercentStep, 0.01f, 1f)));

        amountSpinBox.Step = step;

    }
}
