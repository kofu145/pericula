using Godot;
using Godot.Collections;

public enum Trait { Knight, Arcane, Citizen, Royalty, Mechanical, WildCard, Pawn, Undead };

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
    // current runtime stats
    public int HP;
    public int Attack;

    // encounter stats
    public int BaseAttackAfterCombatBuffs;
    public int BaseHpAfterCombatBuffs;

    // base stats
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

        BaseAttackAfterCombatBuffs = BaseAttack;
        BaseHpAfterCombatBuffs = BaseHP;
        //GD.Print(ToString() + "from CardData");
    }

    public override string ToString()
    {
        return $"{DisplayName} ({id}):\nHealth: {HP}/{BaseHP}, Attack: {Attack}/{BaseAttack}, Trait: {Trait}, Rarity: {Rarity}, Description: {Description}";
    }

    public void ResetForShowdown()
    {
        HP = BaseHpAfterCombatBuffs;
        Attack = BaseAttackAfterCombatBuffs;
        foreach (var effect in OnUse)
        {
            effect.Reset();
        }
        foreach (var effect in Passives)
        {
            effect.Reset();
        }
    }

    public void ResetForEncounter()
    {
        BaseAttackAfterCombatBuffs = BaseAttack;
        BaseHpAfterCombatBuffs = BaseHP;

        ResetForShowdown();
    }

}
