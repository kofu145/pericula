
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
                    if (card.IsTargetTrait(TargetTrait))
                    {
                        count++;
                    }
                }
                var enemies = param.State.OpposingLane(param.Self);
                for (int i = 0; i < count; i++)
                {
                    CardData enemy = null;
                    bool aliveCheck = false;
                    for (int ci = 0; ci < enemies.CardCount; ci++)
                    {
                        if (enemies.GetCardAtIndex(ci).HP > 0)
                        {
                            aliveCheck = true;
                        }
                    }
                    if (!aliveCheck)

                        enemy = enemies.GetCardAtIndex(DeckManager.Instance.RndGen.Next(enemies.CardCount));
                    while (aliveCheck)
                    {
                        enemy = enemies.GetCardAtIndex(DeckManager.Instance.RndGen.Next(enemies.CardCount));
                        if (enemy.HP > 0)
                            break;

                    }
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
