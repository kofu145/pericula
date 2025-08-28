using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CardData : Resource
{
	[Export] public string DisplayName;
	[Export] public string Description;

	[Export(PropertyHint.Enum, "Common,Rare,Mythic,Legendary,Starter,Token")]
	public string Rarity;
	[Export(PropertyHint.Enum, "Knight,Arcane,Citizen,Royalty,Beast,Mechanical,WildCard,Pawn")]
	public string Trait;
	[Export] public int HP;
	[Export] public int MaxHP;
	[Export] public int BaseAttack;
	[Export] public int Attack;
	[Export] public int id;
	[Export] public Image Texture;
	[Export] public Array<EffectTemplate> OnUse;
	[Export] public Array<EffectTemplate> Passives;

	public override string ToString()
	{
		return $"{DisplayName} ({id}):\nHealth: {MaxHP}, Attack: {BaseAttack}, Trait: {Trait}, Rarity: {Rarity}, Description: {Description}";
	}
}
