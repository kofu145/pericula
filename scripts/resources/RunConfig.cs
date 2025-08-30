using Godot;
using System;

[GlobalClass]
public partial class RunConfig : Resource
{
    // player info
    [Export] public int playerStartingChips = 100;
    [Export] public Deck playerStartingDeck;

    // stage info
    [Export] public int enemiesPerAnte;
    [Export] public int antesPerRun;

    // enemy chips info
    [Export] public int baseEnemyStartingChips;
    [Export] public float enemyStartingChipsPerAnteMultipler;

    // Buy In info
    [Export] public int baseBuyIn;
    [Export] public float buyInPerAnteMultiplier;

    // shop prices
    [Export] public int baseRemovalCost;
    [Export] public int RemovalCostIncrease;
    [Export] public int baseUpgradeCost;
    [Export] public int baseCreateRandomCardCost;
    [Export] public int baseDuplicateCost;
}
