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

    protected async Task DamageText(int damage, CardData target, EffectParam param)
    {
        await ToSignal(DeckManager.Instance.GetTree().CreateTimer(.27), Timer.SignalName.Timeout);
        var damagePos = param.State.GetSide(target).GetCardBaseByData(target).GlobalPosition;
        PopupText.Instance.ShowText(damagePos + new Vector2(60, 125), $"-{damage}");
    }

    protected async Task BuffText(int HP, int Attack, CardData target, EffectParam param)
    {
        var cardBase = param.State.GetSide(target).GetCardBaseByData(target);
        //await ToSignal(DeckManager.Instance.GetTree().CreateTimer(.27), Timer.SignalName.Timeout);
        var damagePos = cardBase.GlobalPosition;
        PopupText.Instance.ShowText(damagePos + new Vector2(30, 60), $"+{Attack}/+{HP}");
        cardBase.Visual.UpdateLabels();
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
        ResetAnimation(parent.animation);
    }

    protected async Task DoTriggerAnimation(EffectParam param)
    {
        var parent = param.State.GetSide(param.Self).GetCardBaseByData(param.Self);
        parent.animation.Play("Trigger");
        param.State.ToggleLerp(false);
        await ToSignal(parent.animation, AnimationPlayer.SignalName.AnimationFinished);
        param.State.ToggleLerp(true);
        ResetAnimation(parent.animation);
    }

    private void ResetAnimation(AnimationPlayer anim)
    {
        anim.Play("RESET");
    }

    protected async Task AdvanceInit()
    {
        await ToSignal(EventBus.Instance.GetTree().CreateTimer(.01), "timeout");
        GD.Print("DONE!");
        EventBus.Instance.EmitSignal(EventBus.SignalName.Triggered);
    }

    protected void AdvanceAfterAction()
    {
        EventBus.Instance.RefreshBattle();

    }


}
