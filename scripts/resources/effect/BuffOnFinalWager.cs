using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[GlobalClass]
public partial class BuffOnFinalWager : EffectTemplate
{
    [Export] public int HPBuff;
    [Export] public int AtkBuff;

    [Export] public bool TargetTrait;
    [Export] public Trait TraitToBuff;

    public enum BuffDuration
    { CurrentShowdown, Encounter }
    [Export] public BuffDuration buffDuration;


    private bool ValidTarget(CardData card, EffectParam param) => ((TargetTrait && card.Trait == TraitToBuff) || !TargetTrait) && card != param.Self;

    public override void Initialize(EffectParam param)
    {
        EventBus.FinalWagerHandler handler = null;
        handler = (DeathParam death) =>
        {
            if (death.BaseData != param.Self) return;
            param.State.QueueTrigger(async () =>
            {
                var lane = param.State.GetSide(param.Self);
                for (int i = 0; i < lane.CardCount; i++)
                {
                    var card = lane.GetCardAtIndex(i);
                    if (ValidTarget(card, param))
                    {
                        GD.Print($"Buffing {card.DisplayName}");
                        if (buffDuration == BuffDuration.CurrentShowdown)
                        {
                            card.HP += HPBuff;
                            card.Attack += AtkBuff;
                        }
                        else if (buffDuration == BuffDuration.Encounter)
                        {
                            card.HP += HPBuff;
                            card.Attack += AtkBuff;
                            card.BaseHpAfterCombatBuffs += HPBuff;
                            card.BaseAttackAfterCombatBuffs += AtkBuff;
                        }

                        BuffText(HPBuff, AtkBuff, card, param);

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
