
using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class SummonOnFinalWager : EffectTemplate
{
    [Export] public CardData ToSummonToken;

    public override void Initialize(EffectParam param)
    {
        EventBus.FinalWagerHandler handler = null;
        handler = (DeathParam victim) =>
        {
            param.State.QueueTrigger(async () =>
            {
                var lane = param.State.GetSide(param.Self);
                if (victim == null)
                    return;
                if (true)
                {
                    await DoTriggerAnimation(param);
                    var newUnit = (CardData)ToSummonToken.Duplicate(true);
                    var idx = lane.IndexOf(lane.GetCardBaseByData(param.Self));
                    lane.RemoveCard(param.Self, false);
                    lane.SpawnCard(newUnit, idx);
                }

            });
            EventBus.Instance.FinalWagerEvent -= handler;
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
