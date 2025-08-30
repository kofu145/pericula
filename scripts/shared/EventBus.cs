using Godot;
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

    //[Signal]
    public delegate void EventHandler();

    //public event EventHandler OnDamage;

    public event Action RefreshBattleLoop;
    public event Action ShowdownEvent;
    public event Action AdvantageEvent;
    public event Action UnobscuredEvent;
    public event Action FinalWager;

    public void RefreshBattle() => RefreshBattleLoop?.Invoke();
    public void InvokeAdvantage() => AdvantageEvent?.Invoke();
    public void InvokeObscured() => UnobscuredEvent?.Invoke();
    public void InvokeShowdown() => ShowdownEvent?.Invoke();

    public void ClearEvents()
    {
        RefreshBattleLoop = null;
    }

}
