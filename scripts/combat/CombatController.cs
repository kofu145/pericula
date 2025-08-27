using Godot;
using System.Collections.Generic;

public enum LaneSide { Player, Enemy }

public partial class CombatController : Node
{
	[Export] public Button ShowdownButton;
	[Export] private CardLane playerLane;
	[Export] private CardLane enemyLane;

	private Godot.Collections.Array<Callable> actionQueue = new();
	private BattleState battleState = new();

	public override void _Ready()
	{
		Hide(ShowdownButton);
		ShowdownButton.Pressed += ShowdownHandler;

	}

	public void Initialize()
	{
	}

	public void StartRound(int n)
	{
		DeckManager.Instance.Initialize();
		DeckManager.Instance.Draw(n, true);
		DeckManager.Instance.Draw(n, false);
		GD.Print(DeckManager.Instance.Hand);
		for (int i = 0; i < 2; i++)
		{
			var drawn = i == 0 ? DeckManager.Instance.Hand : DeckManager.Instance.EnemyHand;
			foreach (var data in drawn)
			{
				var lane = i == 0 ? playerLane : enemyLane;
				lane.SpawnCard(data);
			}

		}
		battleState.Initialize(playerLane, enemyLane);
		Show(ShowdownButton);
	}

	public void EndRound()
	{
		playerLane.EndRound();
		enemyLane.EndRound();
		DeckManager.Instance.FinishAndReset();
	}

	public void ShowdownHandler()
	{
		var currLane = battleState.currentTurn == Turn.Player ? playerLane : enemyLane;
		currLane.RemoveCardAtIndex(2);
		//actionQueue.Add(new Callable(this, MethodName.UpdateLanes));
	}

	private void UpdateLanes()
	{

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
}
