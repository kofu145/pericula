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
        EventBus.FinalWagerHandler handler = (CardData? victim) =>
        {
            param.State.QueueTrigger(async () =>
            {
                if (param == null)
                    return;
                if (victim == null)
                    return;
                var lane = param.State.GetSide(param.Self);
                if (lane == null)
                    return;

                GD.Print($"{victim.DisplayName} from lane {param.State.GetSide(victim)} died");
                GD.Print($"unit is in Lane: {lane}");
                GD.Print($"should buff unit: {lane == param.State.GetSide(victim)}");

                if (lane == param.State.GetSide(victim))
                {
                    GD.Print($"buffing unit now");
                    if (Scope == BuffScope.Self)
                    {
                        param.Self.Attack += AtkBuff;
                        param.Self.HP += HPBuff;

                        BuffText(HPBuff, AtkBuff, param.Self, param);
                        await DoTriggerAnimation(param);
                    }
                    else if (Scope == BuffScope.Team)
                    {
                        foreach (var targetBuff in lane.GetAllCardData())
                        {
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

