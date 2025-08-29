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

    [Export] public int baseEnemyStartingChips;
    [Export] public float enemyStartingChipsPerAnteMultipler;

    // Buy In info
    [Export] public int baseBuyIn;
    [Export] public float buyInPerAnteMultiplier;
}
