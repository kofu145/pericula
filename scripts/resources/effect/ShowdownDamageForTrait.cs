
using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class ShowdownDamageForTrait : EffectTemplate
{
    [Export] public int Damage = 2;
    [Export] public Trait TargetTrait = Trait.Arcane;

    public override void Initialize(EffectParam param)
    {
        Action handler = null;
        handler = () =>
        {
            param.State.QueueTrigger(async () =>
            {
                var count = 0;
                var lane = param.State.GetSide(param.Self);
                for (int i = 0; i < lane.CardCount; i++)
                {
                    var card = lane.GetCardAtIndex(i);
                    if (card.Trait == TargetTrait || card.Trait == Trait.WildCard)
                    {
                        count++;
                    }
                }
                var enemies = param.State.OpposingLane(param.Self);
                for (int i = 0; i < count; i++)
                {
                    var enemy = enemies.GetCardAtIndex(DeckManager.Instance.RndGen.Next(enemies.CardCount));
                    DealDamage(Damage, enemy, param);
                    await DoTriggerAnimation(param);
                }

            });
            EventBus.Instance.ShowdownEvent -= handler;
        };

        EventBus.Instance.ShowdownEvent += handler;
    }
    public override async Task OnUse(EffectParam param)
    {
    }

    public async override Task OnEnqueue(EffectParam param)
    {

    }
    public override void Reset() { }
}
