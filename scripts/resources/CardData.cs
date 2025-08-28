using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CardData : Resource
{
    [Export] public string DisplayName;
    [Export] public int HP;
    public int BaseHP;
    public int BaseAttack;
    [Export] public int Attack;
    [Export] public int id;
    [Export] public Image Texture;
    [Export] public Array<EffectTemplate> OnUse;
    [Export] public Array<EffectTemplate> Passives;

    CardData()
    {
        BaseHP = HP;
        BaseAttack = Attack;
    }
}
