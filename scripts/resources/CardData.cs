using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CardData : Resource
{
	[Export] public int id;
	[Export] public string DisplayName;
	[Export] public string Description;

	[Export(PropertyHint.Enum, "Common,Rare,Mythic,Legendary,Starter,Token")]
	public string Rarity;
	[Export(PropertyHint.Enum, "Knight,Arcane,Citizen,Royalty,Beast,Mechanical,WildCard,Pawn")]
	public string Trait;
	public int HP;
	public int Attack;
	[Export] public int BaseAttack;
	[Export] public int BaseHP;
	[Export] public Image Texture;
	[Export] public Array<EffectTemplate> OnUse;
	[Export] public Array<EffectTemplate> Passives;

	CardData()
	{
		BaseHP = HP;
		BaseAttack = Attack;
	}

	public override string ToString()
	{
		return $"{DisplayName} ({id}):\nHealth: {BaseHP}, Attack: {BaseAttack}, Trait: {Trait}, Rarity: {Rarity}, Description: {Description}";
	}

}
