using Godot;
using System;

[GlobalClass]
public partial class SwapLeftWithLowestHealthEnemy : EffectTemplate
{
    [Export] public float swapDuration = 0.25f;
    public override void Initialize(EffectParam param)
    {
        Action handler = null;
        handler = () =>
        {
            param.State.QueueTrigger(
             async () =>
            {
                (int index, int health) currentTarget = (0, int.MaxValue);
                var lane = param.State.OpposingLane(param.Self);
                for (int i = 0; i < lane.CardCount; i++)
                {
                    var card = lane.GetCardAtIndex(i);
                    if (card.HP <= currentTarget.health) currentTarget = (i, card.HP);
                }

                var originalTarget = 0;
                var finalTarget = currentTarget.index;

                var finalCard = lane.GetCardAtIndex(finalTarget);

                param.Self.HP += finalCard.HP;
                param.Self.Attack += finalCard.Attack;

                await BuffText(finalCard.HP, finalCard.Attack, param.Self, param);
                await DoTriggerAnimation(param);
                await lane.Swap(originalTarget, finalTarget, swapDuration);


                // GD.Print($"swapping with {finalTarget}");

            });
            EventBus.Instance.ShowdownEvent -= handler;
        };

        EventBus.Instance.ShowdownEvent += handler;
    }
}
