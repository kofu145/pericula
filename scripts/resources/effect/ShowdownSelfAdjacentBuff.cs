
using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class ShowdownSelfAdjacentBuff : EffectTemplate
{
    [Export] public Trait TraitToBuffSelf = Trait.Mechanical;

    public override void Initialize(EffectParam param)
    {
        Action handler = null;
        handler = () =>
        {
            param.State.QueueTrigger(
             async () =>
            {

                var totalHPBuff = 0;
                var totalAtkBuff = 0;
                var lane = param.State.GetSide(param.Self);
                for (int i = 0; i < lane.CardCount; i++)
                {
                    var card = lane.GetCardAtIndex(i);
                    if (card == param.Self)
                    {
                        CardData target;
                        if (i - 1 >= 0 && (lane.GetCardAtIndex(i - 1).Trait == TraitToBuffSelf || lane.GetCardAtIndex(i - 1).Trait == Trait.WildCard))
                        {
                            target = lane.GetCardAtIndex(i - 1);
                            totalHPBuff += target.HP;
                            totalAtkBuff += target.Attack;

                        }
                        if (i + 1 < lane.CardCount && (lane.GetCardAtIndex(i + 1).Trait == TraitToBuffSelf || lane.GetCardAtIndex(i + 1).Trait == Trait.WildCard))
                        {
                            target = lane.GetCardAtIndex(i + 1);
                            totalHPBuff += target.HP;
                            totalAtkBuff += target.Attack;
                        }

                    }
                }

                param.Self.HP += totalHPBuff;
                param.Self.Attack += totalAtkBuff;
                BuffText(totalHPBuff, totalAtkBuff, param.Self, param);

                await DoTriggerAnimation(param);


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
