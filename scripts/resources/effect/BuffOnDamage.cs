
using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class BuffOnDamage : EffectTemplate
{
    [Export] public int HPBuff = 0;
    [Export] public int AtkBuff = 2;

    public override void Initialize(EffectParam param)
    {
        EventBus.DamageEventHandler handler = (CardData? victim) =>
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

                if (lane == param.State.GetSide(victim))
                {
                    victim.Attack += AtkBuff;
                    victim.HP += HPBuff;
                    var cardBase = lane.GetCardBaseByData(param.Self);
                    if (cardBase.animation.IsPlaying())
                    {
                        await ToSignal(cardBase.animation, AnimationPlayer.SignalName.AnimationFinished);
                    }
                    BuffText(HPBuff, AtkBuff, victim, param);
                    await DoTriggerAnimation(param);
                }


            });

        };

        EventBus.Instance.TakeDamageEvent += handler;
    }
    public override async Task OnUse(EffectParam param)
    {
    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }
    public override void Reset() { }
}
