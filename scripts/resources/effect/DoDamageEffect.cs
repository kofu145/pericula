using Godot;
using System;

[GlobalClass]
public partial class DoDamageEffect : EffectTemplate
{
    [Export] public int Damage;
    public override void OnUse(EffectParam param)
    {
        var opposingLane = param.State.currentTurn == Turn.Enemy ? param.State.PlayerLane : param.State.EnemyLane;

        opposingLane.GetCardAtIndex(0).HP -= Damage;
        var pos = opposingLane.GetCardBaseByData(opposingLane.GetCardAtIndex(0)).GlobalPosition;

        DoAnimation(param, Damage, pos);
        //GD.Print("called damage");
        //DeckManager.Instance.PrintData();

    }

    public async override void OnEnqueue(EffectParam param)
    {

    }

    private async void DoAnimation(EffectParam param, int damage, Vector2 damagePos)
    {
        var isPlayerTurn = param.State.currentTurn == Turn.Player;
        var currLane = isPlayerTurn ? param.State.PlayerLane : param.State.EnemyLane;
        var animName = isPlayerTurn ? "MoveUpAction" : "MoveDownAction";
        var parent = currLane.GetCardBaseByData(param.Self);
        parent.animation.Play(animName);
        //parent.animation.AnimationFinished = null;
        //parent.animation.AnimationFinished += (Godot.StringName animName) => { EventBus.Instance.RefreshBattle(); };
        param.State.ToggleLerp(false);
        await ToSignal(parent.animation, AnimationPlayer.SignalName.AnimationFinished);
        param.State.ToggleLerp(true);
        PopupText.Instance.ShowNumber(damagePos + new Vector2(78, 135), Damage);
        EventBus.Instance.RefreshBattle();
    }

}
