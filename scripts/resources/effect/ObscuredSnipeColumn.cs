using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

[GlobalClass]
public partial class ObscuredSnipeColumn : EffectTemplate
{
    [Export] public int Damage = 10;

    public override void Initialize(EffectParam param)
    {
        param.Self.ObscureChar = true;
        EventBus.ObscuredEventHandler handler = null;
        handler = (List<CardData> list) =>
        {
            param.State.QueueTrigger(async () =>
            {
                if (param == null)
                    return;
                var lane = param.State.GetSide(param.Self);
                if (lane == null)
                    return;
                var count = 0;
                for (int i = 0; i < lane.CardCount; i++)
                {
                    if (lane.GetCardAtIndex(i).ObscureChar)
                        count++;
                }
                var idx = lane.IndexOf(lane.GetCardBaseByData(param.Self));
                if (idx >= 3)
                {
                    var opposingLane = param.State.OpposingLane(param.Self);
                    if (opposingLane.CardCount > idx)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            DealDamage(Damage, opposingLane.GetCardAtIndex(idx), param);
                            GenText("Unobscured!", param.Self, param);
                            await DoTriggerAnimation(param);
                        }

                    }
                }


            });

            EventBus.Instance.ObscuredEvent -= handler;
        };

        EventBus.Instance.ObscuredEvent += handler;
    }
    public override async Task OnUse(EffectParam param)
    {
    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }
    public override void Reset() { }
}

