using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class EffectTemplate : Resource
{

    [Export] public string name;

    /// <summary>
    /// Called after OnUse. Legacy method that you probably won't need to touch.
    /// </summary>
    public async virtual Task OnEnqueue(EffectParam param) { }

    /// <summary>
    /// Called whenever the current unit is actually taking an action on their turn.
    /// (most of the time we will just be using DoDamageEffect)
    /// </summary>
    public async virtual Task OnUse(EffectParam param) { }

    /// <summary>
    /// Called once at the start of the combat phase.
    /// Should use this as a way to set event handlers, NOT to take an actual action.
    /// If you want an example, see showdown usage in ShowdownBuffClass.
    /// </summary>
    public virtual void Initialize(EffectParam param) { }

    /// <summary>
    /// Called when resetting stats of recycled units (discard -> deck).
    /// Should be used to reset event handlers, if need be. (ideally they are self-handled in initialize())
    /// </summary>
    public virtual void Reset() { }

    protected async Task DealDamage(int damage, CardData target, EffectParam param)
    {
        target.HP -= damage;
        await DamageText(damage, target, param);
        EventBus.Instance.InvokeTakeDamageEvent(target);
        //await EventBus.Instance.ClearTriggerQueue();

    }

    /// <summary>
    /// Damage text - timer is aligned perfectly with when the attack animation hits the enemy card.
    /// Don't await this if using in conjunction with an animation.
    /// </summary>
    protected async Task DamageText(int damage, CardData target, EffectParam param)
    {
        var cardBase = param.State.GetSide(target).GetCardBaseByData(target);
        await ToSignal(DeckManager.Instance.GetTree().CreateTimer(.2), Timer.SignalName.Timeout);
        var damagePos = cardBase.GlobalPosition;
        PopupText.Instance.ShowText(damagePos + new Vector2(40, 100), $"-{damage}");
        cardBase.Visual.UpdateLabels();
    }

    /// <summary>
    /// Buff text. May call it over and over on multiple targets.
    /// </summary>
    protected async Task BuffText(int HP, int Attack, CardData target, EffectParam param)
    {
        var cardBase = param.State.GetSide(target).GetCardBaseByData(target);
        await ToSignal(DeckManager.Instance.GetTree().CreateTimer(.05), Timer.SignalName.Timeout);
        var damagePos = cardBase.GlobalPosition;
        PopupText.Instance.ShowText(damagePos + new Vector2(0, 40), $"+{Attack}/+{HP}");
        cardBase.Visual.UpdateLabels();
    }


    /// <summary>
    /// Standard popup text..
    /// </summary>
    protected async Task GenText(string text, CardData target, EffectParam param)
    {
        var cardBase = param.State.GetSide(target).GetCardBaseByData(target);
        await ToSignal(DeckManager.Instance.GetTree().CreateTimer(.05), Timer.SignalName.Timeout);
        var damagePos = cardBase.GlobalPosition;
        PopupText.Instance.ShowText(damagePos + new Vector2(0, 20), text);
        cardBase.Visual.UpdateLabels();
    }

    /// <summary>
    /// Attack animation caller. Await this so that multiple animations don't play at once.
    /// Make sure to call an actual ending method after this, like AdvanceAfterAction()
    /// to actually proceed the gamestate, otherwise the game will softlock.
    /// </summary>
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

    /// <summary>
    /// Trigger animation, for when things like event triggers happen.
    /// Make sure this is awaited, and make sure the end of your callable
    /// calls something like AdvanceInit() (awaited) otherwise the game will hang 
    /// and softlock.
    /// </summary>
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

    /// <summary>
    /// Signal emitter, to let CombatController know we are done processing our action.
    /// Is the only way to proceed the gamestate after an event action.
    /// MUST be called at the end of init event, otherwise game will hang.
    /// </summary>
    protected async Task AdvanceInit()
    {
        //EventBus.Instance.EmitSignal(EventBus.SignalName.Triggered);
        //await ToSignal(EventBus.Instance.GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
    }
    /// <summary>
    /// Calls the combat controller main combat handler again. If this is not called after
    /// an OnUse trigger, the combat manager will literally not refresh or do anything.
    /// MUST be called at the end of OnUse, or game will stop.
    /// </summary>

    protected void AdvanceAfterAction()
    {
        EventBus.Instance.RefreshBattle();

    }



}
