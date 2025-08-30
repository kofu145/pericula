using Godot;
using System;

[GlobalClass]
public partial class RunConfig : Resource
{
    // player info
    [Export] public int playerStartingChips = 100;
    [Export] public Deck playerStartingDeck;

    // stage info
    [Export] public int enemiesPerAnte = 3;
    [Export] public int antesPerRun = 5;

    // enemy chips info

    // Multiplier increase
    // [Export] public int baseEnemyStartingChips = 100;
    // [Export] public int enemyStartingChipsPerAnteIncrease;

    // fixed value
    [Export] public Godot.Collections.Array<int> EnemyChipsPerAnte;
    [Export] public Godot.Collections.Array<int> bossBonusPerAnte;


    // Buy In info
    [Export] public int baseBuyIn;
    [Export] public int buyInIncreasePerAnte;

    // shop prices
    [Export] public int baseRemovalCost;
    [Export] public int baseUpgradeCost;
    [Export] public int baseCreateRandomCardCost;
    [Export] public int baseDuplicateCost;
    [Export] public int rerollCost = 20;
    [Export] public int rerollMultiplierPerUse = 2;

    // when can rarities start to appear
    [Export] public int mythicAvailableAtAnte;
    [Export] public int LegendaryAvailableAtAnte;
}
