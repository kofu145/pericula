using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class DoBuffEffect : EffectTemplate
{
    [Export] public int HPBuff;
    [Export] public int AtkBuff;

    [Export] public bool buffSelf = true;
    [Export] public BuffDuration buffDuration;

    private bool ValidTarget(CardData card, EffectParam param) => buffSelf || (!buffSelf && card != param.Self);

    public override async Task OnUse(EffectParam param)
    {
        var lane = param.State.GetSide(param.Self);

        await DoAttackAnimation(param);
        for (int j = 0; j < lane.CardCount; j++)
        {
            var card = lane.GetCardAtIndex(j);
            if (ValidTarget(card, param))
            {
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
            }
        }
    }
}
