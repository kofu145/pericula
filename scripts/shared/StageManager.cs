using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

public partial class StageManager : Node
{
    public static StageManager Instance { get; private set; }


    // =====================
    // config
    [Export] private RunConfig config;
    private float chipsMultiplierPerAnte;
    // =====================


    // =====================
    // public APIs
    public int AnteCount => config.antesPerRun;
    public int EnemiesPerAnte => config.enemiesPerAnte;
    public bool IsFinalEncounterOfRun => AnteCount - 1 == CurrentAnte && EnemiesPerAnte - 1 == CurrentStageNumber;
    // =====================


    // runtime refs
    // private int currentStage;
    public int CurrentStageNumber { get; private set; }         // 0-indexed
    public int CurrentAnte { get; private set; }                // 0-indexed
    private Godot.Collections.Array<EnemyData> currentAnteEnemies = new();


    [Export] public Godot.Collections.Array<EnemyData> enemiesData;

    public override void _Ready()
    {
        Instance = this;
        CurrentStageNumber = 0;
        CurrentAnte = 0;
    }

    public void StartNewRun()
    {
        // reset the stage and ante
        CurrentAnte = 0;
        CurrentStageNumber = 0;

        // get a random list of enemies
        currentAnteEnemies = GetEnemies();
    }

    public void BeginStage()
    {
        SceneManager.ChangeSceneToFile("Combat");
    }

    public void CompleteStage()
    {
        if (EnemiesPerAnte - 1 == CurrentStageNumber)
        {
            if (AnteCount - 1 == CurrentAnte)
            {
                // Completed Game
                RunEndManager.Instance.WinRun();
                return;
            }

            CurrentStageNumber = 0;
            CurrentAnte++;
            currentAnteEnemies = GetEnemies();
            SceneManager.ChangeSceneToFile("Shop");
            return;
        }

        CurrentStageNumber++;
        SceneManager.ChangeSceneToFile("Shop");
    }

    private Godot.Collections.Array<EnemyData> GetEnemies()
    {
        var nonBosses = new Godot.Collections.Array<EnemyData>();
        EnemyData boss = null;

        // Separate into boss + non-bosses
        foreach (var data in enemiesData)
        {
            if (data.Difficulty == CurrentAnte)
            {
                if (data.IsBoss)
                    boss = data;
                else
                    nonBosses.Add(data);
            }
        }

        // Shuffle non-bosses (Fisher–Yates)
        int n = nonBosses.Count;
        while (n > 1)
        {
            n--;
            int k = DeckManager.Instance.RndGen.Next(n + 1);
            (nonBosses[n], nonBosses[k]) = (nonBosses[k], nonBosses[n]);
        }

        // Take only as many as needed
        var result = new Godot.Collections.Array<EnemyData>();
        int count = Math.Min(nonBosses.Count, EnemiesPerAnte - 1);
        for (int i = 0; i < count; i++)
            result.Add(nonBosses[i]);

        // Always add boss at the end if exists
        if (boss != null)
            result.Add(boss);

        // initialize each cards
        foreach (var enemy in result)
        {
            foreach (CardData card in enemy.deck.Cards)
            {
                card.Initialize();
            }
        }

        return result;
    }
    public EnemyData GetEnemyAtIndex(int i)
    {
        return currentAnteEnemies[i];
    }
    public int GetEnemyStartingChips() => config.EnemyChipsPerAnte[CurrentAnte] + (CurrentStageNumber == EnemiesPerAnte - 1 ? config.bossBonusPerAnte[CurrentAnte] : 0);
    public int GetCurrentBuyIn() => config.baseBuyIn + (CurrentAnte * config.buyInIncreasePerAnte);

    public EnemyData GetCurrentEnemy() => currentAnteEnemies[CurrentStageNumber];
    public Deck GetCurrentEnemyDeck()
    {
        return currentAnteEnemies[CurrentStageNumber].deck;
    }
}
