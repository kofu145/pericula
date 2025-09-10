using Godot;
using System;

[GlobalClass]
public partial class EnemyData : Resource
{
    [Export] public int id;
    [Export] public string DisplayName;
    [Export] public Deck deck;
    [Export] public int Difficulty;
    [Export] public bool IsBoss;
}
