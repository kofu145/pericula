using Godot;
using System;

public partial class RunManager : Node
{
    public static RunManager Instance { get; private set; }
    [Export] private RunEndPanel runEndPanel;

    [Export] private RunConfig runConfig;

    // runtime refs
    public int BetsLost = 0;
    public int BetsWon = 0;
    // public int RemovalUsed = 0;
    // public int DuplicatesUsed = 0;
    // public int UpgradesUsed = 0;
    // public int ConjureUsed = 0;

    public override void _Ready()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }
    }

    public void StartRun()
    {
        Reset();
        ChipManager.Instance.StartNewRun(runConfig.playerStartingChips);
        StageManager.Instance.StartNewRun();
        DeckManager.Instance.StartNewRun(runConfig.playerStartingDeck);
        ShopManager.Instance.StartNewRun();

        SceneManager.ChangeSceneToFile("Shop");
    }

    public void LoseRun()
    {
        runEndPanel.Visible = true;
        runEndPanel.InitializeLoss();
    }

    public void WinRun()
    {
        runEndPanel.Visible = true;
        runEndPanel.InitializeWin();
    }

    public void PlayerLostBet() => BetsLost++;
    public void PlayerWonBet() => BetsWon++;
    // public void PlayerUsedRemoval() => RemovalUsed++;
    // public void PlayerUsedDuplicate() => DuplicatesUsed++;
    // public void PlayerUsedUpgrade() => UpgradesUsed++;
    // public void PlayerUsedConjure() => ConjureUsed++;

    public void Reset()
    {
        BetsLost = 0;
        BetsWon = 0;
        // RemovalUsed = 0;
        // DuplicatesUsed = 0;
        // UpgradesUsed = 0;
        // ConjureUsed = 0;
    }
}
