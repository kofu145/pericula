using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class BuffOnAllyDeath : EffectTemplate
{
    [Export] public int HPBuff = 0;
    [Export] public int AtkBuff = 0;

    public enum BuffScope { Self, Team }
    [Export] public BuffScope Scope = BuffScope.Self;

    public override void Initialize(EffectParam param)
    {
        EventBus.FinalWagerHandler handler = (DeathParam death) =>
        {
            param.State.QueueTrigger(async () =>
            {
                if (param == null)
                    return;
                var lane = param.State.GetSide(param.Self);
                if (lane == null)
                    return;


                if (lane.Side == death.Lane.Side)
                {
                    if (Scope == BuffScope.Self)
                    {
                        param.Self.Attack += AtkBuff;
                        param.Self.HP += HPBuff;

                        BuffText(HPBuff, AtkBuff, param.Self, param);
                        await DoTriggerAnimation(param);
                    }
                    else if (Scope == BuffScope.Team)
                    {
                        for (int i = 0; i < lane.CardCount; i++)
                        {
                            var targetBuff = lane.GetCardAtIndex(i);
                            targetBuff.Attack += AtkBuff;
                            targetBuff.Attack += HPBuff;
                            BuffText(HPBuff, AtkBuff, targetBuff, param);
                            await DoTriggerAnimation(param);
                        }
                    }

                }


            });

        };

        EventBus.Instance.FinalWagerEvent += handler;
    }
    public override async Task OnUse(EffectParam param)
    {
    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }
    public override void Reset() { }
}

