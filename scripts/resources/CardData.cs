using Godot;
using Godot.Collections;

public enum Trait { Knight, Arcane, Citizen, Royalty, Mechanical, WildCard, Pawn, Undead };

[GlobalClass]
public partial class CardData : CodexItemData
{

    [Export]
    public Rarity Rarity;
    [Export]
    public Trait Trait;
    [Export]
    public int AIPriority = 2;
    // current runtime stats
    public int HP;
    public int Attack;

    // encounter stats
    public int BaseAttackAfterCombatBuffs;
    public int BaseHpAfterCombatBuffs;

    public bool ObscureChar = false;

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
    public int GetTraitOrder()
    {
        return Trait switch
        {
            Trait.Pawn => 0,
            Trait.Knight => 1,
            Trait.Royalty => 2,
            Trait.Citizen => 3,
            Trait.Undead => 4,
            Trait.Arcane => 5,
            Trait.Mechanical => 6,
            Trait.WildCard => 7,
            _ => int.MaxValue
        };
    }

    // returns the default sorting order of cards
    public (int rarity, int trait, int id) GetSortKey()
    {
        return ((int)Rarity.RarityType, GetTraitOrder(), id);
    }

    /// <summary>
    /// checks if the targetTrait shares the same trait as the card
    /// </summary>
    /// <param name="targetTrait">The trait to compare to</param>
    /// <returns></returns>
    public bool IsTargetTrait(Trait targetTrait)
    {
        return Trait == targetTrait || Trait == Trait.WildCard;
    }
}
