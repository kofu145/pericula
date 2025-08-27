using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CardData : Resource
{
    [Export] public string DisplayName;
    [Export] public int HP;
    [Export] public int MaxHP;
    [Export] public int BaseAttack;
    [Export] public int Attack;
    [Export] public Image Texture;
    [Export] public Godot.Collections.Array<EffectTemplate> OnUse;
    [Export] public Godot.Collections.Array<EffectTemplate> Passives;
}
