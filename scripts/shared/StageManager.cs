using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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
            SceneManager.ChangeSceneToFile("Shop");
            return;
        }

        CurrentStageNumber++;
        SceneManager.ChangeSceneToFile("Shop");
    }

    private Godot.Collections.Array<EnemyData> GetEnemies()
    {
        Godot.Collections.Array<EnemyData> result = new();
        EnemyData boss = null;
        foreach (var data in enemiesData)
        {
            //GD.Print($"Ante is {CurrentAnte} and data is {data.Difficulty}");
            if (data.Difficulty == CurrentAnte)
            {
                if (data.IsBoss)
                {
                    boss = data;
                }
                else
                    result.Add(data);
            }
        }
        //GD.Print(result);
        // result.Shuffle(); // Better method - shuffle using godot rand 

        // using fisher yates
        int n = result.Count;
        while (n > 1)
        {
            n--;
            int k = DeckManager.Instance.RndGen.Next(n + 1);
            EnemyData val = result[k];
            result[k] = result[n];
            result[n] = val;
        }
        if (boss != null)
            result.Add(boss);
        return result;
    }
    public EnemyData GetEnemyAtIndex(int i)
    {
        GD.Print($"Accessing at {i}");
        return currentAnteEnemies[i];
    }
    public int GetEnemyStartingChips() => config.EnemyChipsPerAnte[CurrentAnte] + (CurrentStageNumber == EnemiesPerAnte - 1 ? config.bossBonusPerAnte[CurrentAnte] : 0);
    public int GetCurrentBuyIn() => config.baseBuyIn + (CurrentAnte * config.buyInIncreasePerAnte);

    public Deck GetCurrentEnemyDeck()
    {
        return currentAnteEnemies[CurrentStageNumber].deck;
    }
}
