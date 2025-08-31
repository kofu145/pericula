using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

[GlobalClass]
public partial class BuffOnObscured : EffectTemplate
{
    [Export] public int HPBuff = 0;
    [Export] public int AtkBuff = 0;

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

                if (lane.IndexOf(lane.GetCardBaseByData(param.Self)) >= 3)
                {
                    param.Self.Attack += AtkBuff;
                    param.Self.HP += HPBuff;

                    BuffText(HPBuff, AtkBuff, param.Self, param);

                    GenText("Unobscured!", param.Self, param);
                    await DoTriggerAnimation(param);

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

