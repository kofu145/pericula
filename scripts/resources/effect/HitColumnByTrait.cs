using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class HitColumnByTrait : EffectTemplate
{
    [Export] public int Damage = 3;
    [Export] public Trait BuffTrait = Trait.Mechanical;

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


                var count = 0;
                for (int i = 0; i < lane.CardCount; i++)
                {
                    var card = lane.GetCardAtIndex(i);
                    if (card.IsTargetTrait(BuffTrait))
                    {
                        count++;
                    }

                }
                if (param.State.GetSide(attacker) == param.State.GetSide(param.Self))
                {
                    var idx = lane.IndexOf(lane.GetCardBaseByData(param.Self));

                    var opposingLane = param.State.OpposingLane(param.Self);
                    if (opposingLane.CardCount > idx)
                    {
                        DealDamage(Damage * count, opposingLane.GetCardAtIndex(idx), param);
                        await DoTriggerAnimation(param);

                    }

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
