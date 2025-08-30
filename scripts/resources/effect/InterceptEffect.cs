using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class InterceptEffect : EffectTemplate
{


    public override void Initialize(EffectParam param)
    {
        param.State.AddToIntercept(param.Self);
    }
    public override async Task OnUse(EffectParam param)
    {
    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }
    public override void Reset() { }
}
