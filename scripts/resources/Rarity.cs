using Godot;
using System;

public enum RarityType {Token, Starter, Common, Rare, Mythic, Legendary}

[GlobalClass]
public partial class Rarity : Resource
{
    public string DisplayName;
    public RarityType RarityType;
    public int ShopCost;
    public float Weights;
    public Color RarityColor;
}
