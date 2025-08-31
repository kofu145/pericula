using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class Execution : EffectTemplate
{
    [Export] public int hitCount = 1;
    public override async Task OnUse(EffectParam param)
    {
        int remainingHits = hitCount;

        while (remainingHits > 0)
        {
            var target = param.State.GetTarget(param.Self);
            if (target == null)
                break;

            await DealDamage(param.Self.Attack, target, param);
            await DoAttackAnimation(param);

            if (target.HP <= 0)
            {
                await EventBus.Instance.UpdateLane();

                // Check if there are still enemies left
                if (param.State.OpposingLane(param.Self).CardCount == 0)
                    break;

                remainingHits++; // "Execution" bonus hit
            }

            remainingHits--;
        }

    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }



}
