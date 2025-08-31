using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class ShowdownScaleBuffClass : EffectTemplate
{
    [Export] public int HPBuff = 2;
    [Export] public int DamageBuff = 2;
    [Export] public Trait TraitToBuff = Trait.Mechanical;
    [Export] public int SpeciesRequirement = 2;

    public override void Initialize(EffectParam param)
    {
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
                    if ((card.Trait == TraitToBuff || card.Trait == Trait.WildCard))
                    {
                        count++;
                    }

                }

                if (count >= SpeciesRequirement)
                {
                    for (int i = 0; i < lane.CardCount; i++)
                    {
                        var member = lane.GetCardAtIndex(i);
                        member.HP += HPBuff * count;
                        member.Attack += DamageBuff * count;
                        BuffText(HPBuff * count, DamageBuff * count, member, param);

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
