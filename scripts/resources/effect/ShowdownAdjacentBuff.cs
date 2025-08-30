using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class ShowdownAdjacentBuff : EffectTemplate
{
    [Export] public int HPBuff = 2;
    [Export] public int DamageBuff = 2;

    public override void Initialize(EffectParam param)
    {
        Action handler = null;
        handler = () =>
        {
            param.State.QueueTrigger(
             async () =>
            {
                var lane = param.State.GetSide(param.Self);
                for (int i = 0; i < lane.CardCount; i++)
                {
                    var card = lane.GetCardAtIndex(i);
                    if (card == param.Self)
                    {
                        CardData target;
                        if (i - 1 >= 0)
                        {
                            target = lane.GetCardAtIndex(i - 1);
                            target.HP += HPBuff;
                            target.Attack += DamageBuff;
                            BuffText(HPBuff, DamageBuff, target, param);

                            await DoTriggerAnimation(param);
                        }
                        if (i + 1 < lane.CardCount)
                        {
                            target = lane.GetCardAtIndex(i + 1);
                            target.HP += HPBuff;
                            target.Attack += DamageBuff;
                            BuffText(HPBuff, DamageBuff, target, param);

                            await DoTriggerAnimation(param);
                        }

                    }
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
