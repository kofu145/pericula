using Godot;
using System;

[GlobalClass]
public partial class EnemyData : Resource
{
    [Export] public string DisplayName;
    [Export] public Deck deck;
}
