using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class EffectTemplate : Resource
{

    [Export] public string name;
    public async virtual void OnEnqueue(EffectParam param) { }

    public async virtual void OnUse(EffectParam param) { }

    public virtual void Initialize(EffectParam param) { }

    public virtual void Reset() { }

    protected void DamageText(int damage, CardData target, EffectParam param)
    {
        var damagePos = param.State.GetSide(target).GetCardBaseByData(target).GlobalPosition;
        PopupText.Instance.ShowText(damagePos + new Vector2(70, 130), $"-{damage}");
    }

    protected async Task DoAttackAnimation(EffectParam param)
    {
        var currLane = param.State.GetSide(param.Self);
        var animName = currLane.Side == LaneSide.Player ? "MoveUpAction" : "MoveDownAction";
        var parent = currLane.GetCardBaseByData(param.Self);
        parent.animation.Play(animName);
        param.State.ToggleLerp(false);
        await ToSignal(parent.animation, AnimationPlayer.SignalName.AnimationFinished);
        param.State.ToggleLerp(true);
    }

    protected async Task DoTriggerAnimation(EffectParam param)
    {
        var parent = param.State.GetSide(param.Self).GetCardBaseByData(param.Self);
        parent.animation.Play("Trigger");
        param.State.ToggleLerp(false);
        await ToSignal(parent.animation, AnimationPlayer.SignalName.AnimationFinished);
        param.State.ToggleLerp(true);
    }

    protected void AdvanceAfterAction()
    {
        EventBus.Instance.RefreshBattle();

    }


}
