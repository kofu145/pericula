using Godot;
using System;
using System.Collections.Generic;

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
        Dictionary<int, int> deck = new();
        foreach (var card in DeckManager.Instance.PlayerDeck.Cards)
        {
            if (deck.ContainsKey(card.id)) deck[card.id]++;
            else deck[card.id] = 1;
        }
        var enemy = StageManager.Instance.GetCurrentEnemy().id.ToString();
        var ver = ProjectSettings.GetSetting("application/config/version").AsString();
        AnalyticsManager.Instance.LogRun(deck, false, enemy, ver);

        runEndPanel.Visible = true;
        runEndPanel.InitializeLoss();
    }

    public void WinRun()
    {
        Dictionary<int, int> deck = new();
        foreach (var card in DeckManager.Instance.PlayerDeck.Cards)
        {
            if (deck.ContainsKey(card.id)) deck[card.id]++;
            else deck[card.id] = 1;
        }
        var ver = ProjectSettings.GetSetting("application/config/version").AsString();
        AnalyticsManager.Instance.LogRun(deck, true, clientVersion: ver);

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
