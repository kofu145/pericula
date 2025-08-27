using Godot;
using System.Collections.Generic;

public enum LaneSide { Player, Enemy }

public partial class CombatController : Node
{
	[Export] public Button ShowdownButton;
	[Export] private CardLane playerLane;
	[Export] private CardLane enemyLane;

	public override void _Ready()
	{
		Hide(ShowdownButton);
	}

	public void Initialize()
	{
	}

	public void StartRound(int n)
	{
		DeckManager.Instance.Draw(n, true);
		DeckManager.Instance.Draw(n, false);
		for (int i = 0; i < 2; i++)
		{
			var drawn = i == 0 ? DeckManager.Instance.Hand : DeckManager.Instance.EnemyHand;
			foreach (var data in drawn)
			{
				var lane = i == 0 ? playerLane : enemyLane;
				lane.SpawnCard(data);
			}

		}
		Show(ShowdownButton);
	}

	public void EndRound()
	{
		playerLane.EndRound();
		enemyLane.EndRound();
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
