
using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class OnDamageDealRandomDamage : EffectTemplate
{
    [Export] public int Damage = 8;

    public override void Initialize(EffectParam param)
    {
        EventBus.DamageEventHandler handler = (CardData? victim) =>
        {
            if (param == null)
                return;

            param.State.QueueTrigger(async () =>
            {
                var lane = param.State.GetSide(param.Self);
                if (lane == null)
                    return;
                if (lane.CardCount > 0 && victim == param.Self)
                {
                    var cardBase = lane.GetCardBaseByData(param.Self);
                    if (cardBase.animation.IsPlaying())
                    {
                        await ToSignal(cardBase.animation, AnimationPlayer.SignalName.AnimationFinished);
                    }
                    var opposed = param.State.OpposingLane(param.Self);
                    await DealDamage(Damage, opposed.GetCardAtIndex(DeckManager.Instance.RndGen.Next(opposed.CardCount)), param);
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
