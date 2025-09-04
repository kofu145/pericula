
using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class RetriggerSameTrait : EffectTemplate
{
    [Export]
    public Trait TraitToRetrigger;
    private EffectParam param;

    public override void PreInit(EffectParam param)
    {
        this.param = param;

        Reset();
        var side = param.State.GetSide(param.Self);
        var idx = side.IndexOf(side.GetCardBaseByData(param.Self));
        try
        {
            for (int i = 0; i < side.CardCount; i++)
            {
                var card = side.GetCardAtIndex(i);
                if (card.IsTargetTrait(TraitToRetrigger) && card.id != param.Self.id)
                {
                    int copyId = card.id;
                    var copyBase = (CardData)Lookup.GetCardByID(copyId).Duplicate(true);
                    param.Self.Passives.AddRange(copyBase.Passives.Duplicate(true));
                }
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
            try
            {
                while (param.Self.Passives.Count > 1)
                {
                    param.Self.Passives.RemoveAt(param.Self.Passives.Count - 1);
                }
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"err in preinit {ex.Message}");
                GD.PushError($"Full error details: {ex.ToString()}");
            }
        }

    }
}
