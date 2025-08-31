using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class AttackOnAdvantage : EffectTemplate
{
    [Export] public int Damage = 3;

    public override void Initialize(EffectParam param)
    {
        EventBus.AdvantageEventHandler handler = null;
        handler = (CardData? attacker) =>
        {
            param.State.QueueTrigger(async () =>
            {
                var lane = param.State.GetSide(param.Self);
                if (lane == null)
                    return;
                if (lane.CardCount <= 0)
                    return;
                if (param.State.GetSide(attacker) == param.State.GetSide(param.Self))
                {
                    var target = param.State.OpposingLane(param.Self).GetCardAtIndex(DeckManager.Instance.RndGen.Next(param.State.OpposingLane(param.Self).CardCount));
                    DealDamage(Damage, target, param);
                    await DoTriggerAnimation(param);
                }



            });
        };

        EventBus.Instance.AdvantageEvent += handler;
    }
    public override async Task OnUse(EffectParam param)
    {
    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }
    public override void Reset() { }
}
