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
        PopupText.Instance.ShowNumber(pos + new Vector2(10, -30), Damage);
        //GD.Print("called damage");

    }

    public async override void OnEnqueue(EffectParam param)
    {
        var isPlayerTurn = param.State.currentTurn == Turn.Player;
        var currLane = isPlayerTurn ? param.State.PlayerLane : param.State.EnemyLane;
        var animName = isPlayerTurn ? "MoveUpAction" : "MoveDownAction";
        var parent = currLane.GetCardBaseByData(param.Self);
        parent.animation.Play(animName);
        //parent.animation.AnimationFinished = null;
        //parent.animation.AnimationFinished += (Godot.StringName animName) => { EventBus.Instance.RefreshBattle(); };

        await ToSignal(parent.animation, AnimationPlayer.SignalName.AnimationFinished);
        EventBus.Instance.RefreshBattle();
    }

}
