using Godot;
using System;

[GlobalClass]
public partial class PawnAdvantageBuffEffect : EffectTemplate
{
    [Export] public int HPBuff = 2;
    [Export] public int DamageBuff = 2;
    public override void Initialize(EffectParam param)
    {
        GD.Print("passive called!");
        Action handler = null;
        handler = () =>
        {
            Action action = async () =>
            {
                GD.Print("other thing called");
                param.Self.HP += HPBuff;
                param.Self.Attack += DamageBuff;
                BuffText(HPBuff, DamageBuff, param.Self, param);
                await DoTriggerAnimation(param);

                EventBus.Instance.EmitSignal(EventBus.SignalName.Triggered);
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
