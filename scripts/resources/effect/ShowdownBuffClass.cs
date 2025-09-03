using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class ShowdownBuffClass : EffectTemplate
{
    [Export] public int HPBuff = 1;
    [Export] public int DamageBuff = 1;
    [Export] public Trait TraitToBuff = Trait.Arcane;
    [Export] public bool CanTargetSelf = true;

    public override void Initialize(EffectParam param)
    {
        Action handler = null;
        handler = () =>
        {
            param.State.QueueTrigger(async () =>
            {
                var lane = param.State.GetSide(param.Self);
                for (int i = 0; i < lane.CardCount; i++)
                {
                    var card = lane.GetCardAtIndex(i);
                    if (card.IsTargetTrait(TraitToBuff) && (CanTargetSelf || card != param.Self))
                    {
                        card.HP += HPBuff;
                        card.Attack += DamageBuff;
                        BuffText(HPBuff, DamageBuff, card, param);

                        await DoTriggerAnimation(param);
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
