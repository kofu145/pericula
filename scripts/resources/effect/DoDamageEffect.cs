using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class DoDamageEffect : EffectTemplate
{
    public override async Task OnUse(EffectParam param)
    {
        var target = param.State.GetTarget(param.Self);
        await DealDamage(param.Self.Attack, target, param);
        await DoAttackAnimation(param);
        AdvanceAfterAction();
    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }



}
