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

    private Queue<Callable> triggerQueue = new();
    private Godot.Collections.Array<CardData> hasIntercept = new();

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

    public CardLane? GetSide(CardData card)
    {
        if (PlayerLane.Contains(card))
            return PlayerLane;
        else if (EnemyLane.Contains(card))
            return EnemyLane;

        return null;
    }

    public async Task PopTriggerQueue()
    {
        triggerQueue.Dequeue().Call();
        await ToSignal(EventBus.Instance, EventBus.SignalName.Triggered);
    }

    public void QueueTrigger(Callable callable)
    {
        triggerQueue.Enqueue(callable);
    }
}
