using Godot;
using System;

public partial class BattleState : Node
{
    private int playerBet = 0;
    private int enemyBet = 0;
    public int Pot => playerBet + enemyBet;
    public Godot.Collections.Array<CardData> PlayerLane;
    public Godot.Collections.Array<CardData> EnemyLane;
}
