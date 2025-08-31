using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class DoDamageEffect : EffectTemplate
{
    [Export] public int hitCount = 1;
    public override async Task OnUse(EffectParam param)
    {
        for (int i = 0; i < hitCount; i++)
        {
            var target = param.State.GetTarget(param.Self);
            DealDamage(param.Self.Attack, target, param);

            await DoAttackAnimation(param);

        }
    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }



}
