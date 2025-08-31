
using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class BuffOnFinalWager : EffectTemplate
{
    [Export] public Trait TraitToBuff;
    [Export] public int HPBuff = 1;
    [Export] public int DamageBuff = 1;

    public override void Initialize(EffectParam param)
    {
        EventBus.FinalWagerHandler handler = null;
        handler = (CardData? victim) =>
        {
            param.State.QueueTrigger(async () =>
            {
                var lane = param.State.GetSide(param.Self);
                if (victim == null)
                    return;
                GD.Print($"self: {param.Self.DisplayName}, {victim.DisplayName}");
                for (int i = 0; i < lane.CardCount; i++)
                {
                    var card = lane.GetCardAtIndex(i);
                    if ((TraitToBuff == Trait.All || TraitToBuff == card.Trait) && card != param.Self)
                    {
                        card.HP += HPBuff;
                        card.Attack += DamageBuff;
                        BuffText(HPBuff, DamageBuff, card, param);

                        await DoTriggerAnimation(param);
                    }
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
