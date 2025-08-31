using Godot;
using System;

public partial class ShowdownGiveExtraAttackAt : EffectTemplate
{
    [Export] public int targetIndex = 0;

    public override void Initialize(EffectParam param)
    {
        Action handler = null;
        handler = () =>
        {
            param.State.QueueTrigger(
             async () =>
            {
                var lane = param.State.GetSide(param.Self);
                var card = lane.GetCardAtIndex(targetIndex);
                // card.
                // give attack 

                await DoTriggerAnimation(param);
            });
            EventBus.Instance.ShowdownEvent -= handler;
        };

        EventBus.Instance.ShowdownEvent += handler;
    }
}
