using Godot;
using System;

[GlobalClass]
public partial class PawnAdvantageBuffEffect : EffectTemplate
{
    [Export] public int HPBuff = 2;
    [Export] public int DamageBuff = 2;
    public override void Initialize(EffectParam param)
    {
        Action handler = null;
        handler = () =>
        {
            Action action = async () =>
            {
                var lane = param.State.GetSide(param.Self);
                bool pawnExists = false;
                for (int i = 0; i < lane.CardCount; i++)
                {
                    if (lane.GetCardAtIndex(i).id == 4 && lane.GetCardAtIndex(i) != param.Self)
                        pawnExists = true;
                }
                if (pawnExists)
                {
                    GD.Print("exists so doing the thing!");
                    param.Self.HP += HPBuff;
                    param.Self.Attack += DamageBuff;
                    BuffText(HPBuff, DamageBuff, param.Self, param);

                    await DoTriggerAnimation(param);
                }

                await AdvanceInit();

            };
            param.State.QueueTrigger(Callable.From(action));
            EventBus.Instance.ShowdownEvent -= handler;
        };

        EventBus.Instance.ShowdownEvent += handler;
    }
    public override async void OnUse(EffectParam param)
    {
    }

    public async override void OnEnqueue(EffectParam param)
    {

    }
    public override void Reset() { }
}
