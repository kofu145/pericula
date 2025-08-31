using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class BuffOnAdvantage : EffectTemplate
{
    [Export] public int HPBuff = 0;
    [Export] public int DamageBuff = 2;

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

                var targetAlly = lane.GetCardAtIndex(DeckManager.Instance.RndGen.Next(lane.CardCount));
                targetAlly.HP += HPBuff;
                targetAlly.Attack += DamageBuff;
                BuffText(HPBuff, DamageBuff, targetAlly, param);
                await DoTriggerAnimation(param);

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
