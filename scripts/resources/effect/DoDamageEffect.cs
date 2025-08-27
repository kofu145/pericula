using Godot;
using System;

[GlobalClass]
public partial class DoDamageEffect : EffectTemplate
{
	[Export] public int Damage;
	public override void OnUse(EffectParam param)
	{
		param.Target.HP -= Damage;
	}
}
