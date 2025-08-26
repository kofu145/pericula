using Godot;
using System;

[GlobalClass]
public partial class CardData : Resource
{
    [Export] public string DisplayName;
    [Export] public int HP;
    [Export] public int Attack;
}
