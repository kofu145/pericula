using Godot;
using System;

public partial class BattleState : Node
{
	private int playerBet = 0;
	private int enemyBet = 0;
	public int Pot => playerBet + enemyBet;
	public CardLane PlayerLane;
	public CardLane EnemyLane;
	public Turn currentTurn;

	public void Initialize(CardLane playerLane, CardLane enemyLane)
	{
		PlayerLane = playerLane;
		EnemyLane = enemyLane;
		currentTurn = Turn.Player;
	}
}
