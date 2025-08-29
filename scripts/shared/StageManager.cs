using Godot;
using System;

public partial class StageManager : Node
{
	public int CurrentStageID { get; private set; }
	public static StageManager Instance { get; private set; }

	const int TOTAL_STAGE_COUNT = 10;

	public override void _Ready()
	{
		Instance = this;
		CurrentStageID = 1;
	}

	public void StartNewRun()
	{
		CurrentStageID = 1;
	}

	public void BeginStage()
	{
		SceneManager.ChangeSceneToFile("Combat");
	}

	public void CompleteStage()
	{
		if (CurrentStageID >= TOTAL_STAGE_COUNT)
		{
			// Completed Game
			GD.Print("You completed the game");
		}

		CurrentStageID++;
		SceneManager.ChangeSceneToFile("Shop");
	}
}
