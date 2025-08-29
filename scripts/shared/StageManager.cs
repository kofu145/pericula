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
    private List<EnemyData> currentAnteEnemies = new();


    // TODO: Temporary implementation, remove when GetEnemies() is implemented
    [Export] private EnemyData tempEnemy;

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

    // TODO: based on the currentAnte, returns a list of EnemiesPerAnte number of Enemies
    private List<EnemyData> GetEnemies()
    {
        List<EnemyData> tempList = new();
        for (int i = 0; i < EnemiesPerAnte; i++)
        {
            tempList.Add(tempEnemy);
        }
        return tempList;
    }
    public EnemyData GetEnemyAtIndex(int i)
    {
        return currentAnteEnemies[i];
    }
    public int GetEnemyStartingChips()
    {
        // TODO: no multiplier based on ante yet
        return config.baseEnemyStartingChips;
    }
    public int GetCurrentBuyIn()
    {
        // TODO: no multiplier based on ante yet
        return config.baseBuyIn;
    }
    public Deck GetCurrentEnemyDeck()
    {
        return currentAnteEnemies[CurrentStageNumber].deck;
    }
}
