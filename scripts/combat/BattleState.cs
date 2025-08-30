using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class BattleState : Node
{
    private int playerBet = 0;
    private int enemyBet = 0;
    public int Pot => playerBet + enemyBet;
    public CardLane PlayerLane;
    public CardLane EnemyLane;
    public Turn currentTurn;
    public int QueueCount => triggerQueue.Count;

    private Queue<Func<Task>> triggerQueue = new();
    private List<CardData> hasIntercept = new();

    public void Initialize(CardLane playerLane, CardLane enemyLane)
    {
        PlayerLane = playerLane;
        EnemyLane = enemyLane;
        currentTurn = Turn.Player;
    }

    public void ToggleLerp(bool value)
    {
        PlayerLane.ToggleVisualLerp(value);
        EnemyLane.ToggleVisualLerp(value);
    }

    public CardData GetTarget(CardData self)
    {
        hasIntercept.RemoveAll(e => e == null); // doubt this is neccesary since carddata isnt being nulled
        var selfLane = GetSide(self);
        var opposingLane = selfLane.Side == LaneSide.Player ? EnemyLane : PlayerLane;
        CardData target = opposingLane.GetCardAtIndex(0);
        var found = false;
        for (int i = 0; i < opposingLane.CardCount; i++)
        {
            foreach (var card in hasIntercept)
            {
                if (opposingLane.GetCardAtIndex(i) == card)
                {
                    target = card;
                    break;

                }
            }
            if (found)
                break;
        }

        return target;
    }

    public void AddToIntercept(CardData card)
    {
        hasIntercept.Add(card);
    }

    public CardLane? GetSide(CardData card)
    {
        if (PlayerLane.Contains(card))
            return PlayerLane;
        else if (EnemyLane.Contains(card))
            return EnemyLane;

        return null;
    }

    public CardLane OpposingLane(CardData card) => GetSide(card).Side == LaneSide.Player ? EnemyLane : PlayerLane;

    public async Task PopTriggerQueue()
    {
        if (triggerQueue.Count == 0)
            return;
        var func = triggerQueue.Dequeue();
        //await ToSignal(EventBus.Instance, EventBus.SignalName.Triggered);
        await func();
        //await ToSignal(EventBus.Instance.GetTree().CreateTimer(0.7f), SceneTreeTimer.SignalName.Timeout);
    }

    public void QueueTrigger(Func<Task> func)
    {
        triggerQueue.Enqueue(func);
    }
}
