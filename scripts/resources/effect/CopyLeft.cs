using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class CopyLeft : EffectTemplate
{
    private EffectParam param;

    public override void PreInit(EffectParam param)
    {
        this.param = param;

        Reset();
        GD.Print("start of pre init");
        var side = param.State.GetSide(param.Self);
        var idx = side.IndexOf(side.GetCardBaseByData(param.Self));
        try
        {
            if (idx - 1 >= 0 && side.GetCardAtIndex(idx - 1).id != param.Self.id)
            {
                int copyId = side.GetCardAtIndex(idx - 1).id;
                var copyBase = (CardData)Lookup.GetCardByID(copyId).Duplicate(true);
                param.Self.OnUse.AddRange(copyBase.OnUse.Duplicate(true));
                param.Self.Passives.AddRange(copyBase.Passives.Duplicate(true));
                param.Self.Attack = copyBase.BaseAttack;
                param.Self.HP = copyBase.BaseHP;
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"err in preinit {ex.Message}");
            GD.PushError($"Full error details: {ex.ToString()}");
        }
        GD.Print("end of pre in it");
    }

    public override void Initialize(EffectParam param)
    {


    }
    public override async Task OnUse(EffectParam param)
    {
    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }
    public override void Reset()
    {
        if (param != null)
        {
            param.Self.Passives.Clear();
            param.Self.OnUse.Clear();

            param.Self.Passives.Add(new CopyLeft());
            param.Self.Attack = 1;
            param.Self.HP = 1;
        }

    }
}
