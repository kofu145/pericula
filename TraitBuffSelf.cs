using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class TraitBuffSelf : EffectTemplate
{
    [Export] public int HPBuff = 5;
    [Export] public int DamageBuff = 5;
    [Export] public Trait TraitToBuff = Trait.Citizen;
    [Export] public int InterceptThreshold = 2;

    public override void Initialize(EffectParam param)
    {
        var count = 0;
        var lane = param.State.GetSide(param.Self);
        for (int i = 0; i < lane.CardCount; i++)
        {
            var card = lane.GetCardAtIndex(i);
            if ((card.Trait == TraitToBuff || card.Trait == Trait.WildCard) && card != param.Self)
            {
                count++;
            }
        }
        if (count >= InterceptThreshold)
            param.State.AddToIntercept(param.Self);
        Action handler = null;
        handler = () =>
        {
            param.State.QueueTrigger(async () =>
            {
                var count = 0;
                var lane = param.State.GetSide(param.Self);
                for (int i = 0; i < lane.CardCount; i++)
                {
                    var card = lane.GetCardAtIndex(i);
                    if (card.Trait == TraitToBuff && card != param.Self)
                    {
                        count++;
                    }
                }
                if (count >= 2)
                {
                    param.Self.HP += HPBuff;
                    param.Self.Attack += DamageBuff;
                    BuffText(HPBuff, DamageBuff, param.Self, param);

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
