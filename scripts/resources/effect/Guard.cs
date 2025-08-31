using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class Guard : EffectTemplate
{
    private bool isPlayer;
    private EffectParam param;

    public override void Initialize(EffectParam param)
    {
        isPlayer = param.State.GetSide(param.Self).Side == LaneSide.Player;
        if (isPlayer)
            param.State.PlayerDefense += 1;
        else
        {
            param.State.EnemyDefense += 1;
        }
        this.param = param;
    }
    public override async Task OnUse(EffectParam param)
    {
    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }
    public override void Reset()
    {

    }
}
