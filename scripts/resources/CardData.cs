using Godot;
using Godot.Collections;

public enum Trait { Knight, Arcane, Citizen, Royalty, Beast, Mechanical, WildCard, Pawn };

[GlobalClass]
public partial class CardData : Resource
{
    [Export] public int id;
    [Export] public string DisplayName;
    [Export(PropertyHint.MultilineText)] public string Description;

    [Export]
    public Rarity Rarity;
    [Export]
    public Trait Trait;
    public int HP;
    public int Attack;
    [Export] public int BaseAttack;
    [Export] public int BaseHP;
    [Export] public Texture2D Texture;
    [Export] public Array<EffectTemplate> OnUse;
    [Export] public Array<EffectTemplate> Passives;
    [Export] public Array<Keyword> Keywords = new Array<Keyword>();

    public void Initialize()
    {
        HP = BaseHP;
        Attack = BaseAttack;
        //GD.Print(ToString() + "from CardData");
    }

    public override string ToString()
    {
        return $"{DisplayName} ({id}):\nHealth: {HP}/{BaseHP}, Attack: {Attack}/{BaseAttack}, Trait: {Trait}, Rarity: {Rarity}, Description: {Description}";
    }

    public void ResetForBattle()
    {
        HP = BaseHP;
        Attack = BaseAttack;
        foreach (var effect in OnUse)
        {
            effect.Reset();
        }
        foreach (var effect in Passives)
        {
            effect.Reset();
        }
    }

}
