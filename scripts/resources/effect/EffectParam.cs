using System;

public class EffectParam
{
    public BattleState State;
    public CardData Target;
    public CardData Self;
    public EffectParam(BattleState state, CardData target, CardData self)
    {
        State = state;
        Target = target;
        Self = self;
    }
}
