using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class ShowdownBuffPosition : EffectTemplate
{
    [Export] public int HPBuff = 4;
    [Export] public int AtkBuff = 4;
    [Export] public int PositionToBuff = 0;

    public override void Initialize(EffectParam param)
    {
        Action handler = null;
        handler = () =>
        {

            param.State.QueueTrigger(
            async () =>
            {
                var lane = param.State.GetSide(param.Self);
                if (lane.CardCount > 0)
                {

                    var target = lane.GetCardAtIndex(PositionToBuff);
                    target.HP += HPBuff;
                    target.Attack += AtkBuff;
                    BuffText(HPBuff, AtkBuff, target, param);
                    await DoTriggerAnimation(param);
                }
            });
            EventBus.Instance.ShowdownEvent -= handler;
        };

        EventBus.Instance.ShowdownEvent += handler;
    }
    public override async Task OnUse(EffectParam param)
    {
    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }
    public override void Reset() { }
}
