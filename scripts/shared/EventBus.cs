using Godot;
using System.Threading.Tasks;
using System;

public partial class EventBus : Node
{
    public static EventBus Instance { get; private set; }
    public override void _Ready()
    {
        Instance = this;
    }

    [Signal]
    public delegate void TriggeredEventHandler();
    public CombatController CombatManager;


    //[Signal]
    public delegate void EventHandler();

    public delegate void DamageEventHandler(CardData? victim);
    public delegate void AdvantageEventHandler(CardData? attacker);
    public delegate void FinalWagerHandler(DeathParam death);

    //public event EventHandler OnDamage;

    public event Action RefreshBattleLoop;
    public event Action ShowdownEvent;
    public event DamageEventHandler TakeDamageEvent;
    public event AdvantageEventHandler AdvantageEvent;
    public event Action UnobscuredEvent;
    public event FinalWagerHandler FinalWagerEvent;
    public event Action ClearTriggerQueueEvent;

    public void RefreshBattle() => CombatManager.BattleRefreshHandler();
    public void InvokeTakeDamageEvent(CardData? card) => TakeDamageEvent?.Invoke(card);
    public void InvokeAdvantage(CardData? attacker) => AdvantageEvent?.Invoke(attacker);
    public void InvokeObscured() => UnobscuredEvent?.Invoke();
    public void InvokeShowdown() => ShowdownEvent?.Invoke();
    public void InvokeFinalWager(DeathParam death) => FinalWagerEvent?.Invoke(death);
    public async Task ClearTriggerQueue() => await CombatManager.ClearActionQueue();

    public void ClearEvents()
    {
        ShowdownEvent = null;
        TakeDamageEvent = null;
        AdvantageEvent = null;
        UnobscuredEvent = null;
        FinalWagerEvent = null;
        ClearTriggerQueueEvent = null;
        RefreshBattleLoop = null;
    }



}
