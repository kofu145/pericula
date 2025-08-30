using Godot;
using System;

public enum RarityType {Token, Starter, Common, Rare, Mythic, Legendary}

[GlobalClass]
public partial class Rarity : Resource
{
    [Export] public string DisplayName;
    [Export] public RarityType RarityType;
    [Export] public int ShopCost;
    [Export] public float Weights;
    [Export] public Color RarityColor;
}
