using Godot;
using System;

[GlobalClass]
public partial class DoDamageEffect : EffectTemplate
{
    public override async void OnUse(EffectParam param)
    {
        var target = param.State.GetTarget(param.Self);
        target.HP -= param.Self.Attack;
        DamageText(param.Self.Attack, target, param);
        await DoAttackAnimation(param);
        AdvanceAfterAction();
    }

    public async override void OnEnqueue(EffectParam param)
    {

    }



}
