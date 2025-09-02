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
    public int PlayerDefense;
    public int EnemyDefense;

    public void Initialize(CardLane playerLane, CardLane enemyLane)
    {
        PlayerLane = playerLane;
        EnemyLane = enemyLane;
        currentTurn = Turn.Player;
        PlayerDefense = 0;
        EnemyDefense = 0;
    }

    public void Reset()
    {
        PlayerDefense = 0;
        EnemyDefense = 0;
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
        if (hasIntercept.Count > 0)
        {
            CardData leftmostIntercept = null;
            int lowestIndex = int.MaxValue;

            foreach (var card in hasIntercept)
            {
                if (opposingLane.GetCardBaseByData(card) != null)
                {
                    int idx = opposingLane.IndexOf(opposingLane.GetCardBaseByData(card));
                    if (idx >= 0 && idx < lowestIndex) // make sure the card is actually in lane
                    {
                        lowestIndex = idx;
                        leftmostIntercept = card;
                    }
                }

            }

            if (leftmostIntercept != null)
                target = leftmostIntercept;
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

    public void UpdateLabels()
    {
        for (int i = 0; i < 2; i++)
        {
            var targetLane = i == 0 ? PlayerLane : EnemyLane;
            for (int j = 0; j < targetLane.CardCount; j++)
            {
                targetLane.GetBaseAtIndex(j).Visual.UpdateLabels();
            }
        }
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
