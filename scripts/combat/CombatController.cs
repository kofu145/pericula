using Godot;
using System;
using System.Collections.Generic;

public enum LaneSide { Player, Enemy }

public partial class CombatController : Node
{
    [Export] private CardLane playerLane;
    [Export] private CardLane enemyLane;

    public Action<bool> OnShowdownEndPlayerWin;

    private Godot.Collections.Array<Callable> actionQueue = new();
    private Godot.Collections.Array<Callable> animationQueue = new();
    private Godot.Collections.Array<EffectParam> queueParams = new();
    private BattleState battleState = new();

    public void Initialize()
    {
        DeckManager.Instance.Initialize();
    }

    public void StartRound(int n)
    {
        DeckManager.Instance.Draw(n, true);
        DeckManager.Instance.Draw(n, false);
        //GD.Print(DeckManager.Instance.Hand + "From StartRound in CombatController");
        //GD.Print(DeckManager.Instance.EnemyHand);
        for (int i = 0; i < 2; i++)
        {
            var drawn = i == 0 ? DeckManager.Instance.Hand : DeckManager.Instance.EnemyHand;
            GD.Print(drawn.Count);
            for (int j = 0; j < drawn.Count; j++)
            {
                var lane = i == 0 ? playerLane : enemyLane;
                lane.SpawnCard(drawn[j], j);
            }

        }
        playerLane.SetFlopPhase();
        enemyLane.SetFlopPhase();
        battleState.Initialize(playerLane, enemyLane);
    }

    /// <summary>
    /// Indicate to combat manager that bet phase has started.
    /// </summary>
    public void StartBetPhase()
    {
        playerLane.SetBetPhase();
        enemyLane.SetBetPhase();
    }

    /// <summary>
    /// Ends the current turn in combat, enemy and player both discard their remaining hands.
    /// </summary>
    public void EndTurn()
    {
        playerLane.EndRound();
        enemyLane.EndRound();
        EventBus.Instance.ClearEvents();
    }

    /// <summary>
    /// Ends the current combat encounter. Clearing the player and enemy decks.
    /// </summary>
    public void EndCombat()
    {
        playerLane.EndRound();
        enemyLane.EndRound();

        DeckManager.Instance.FinishAndReset();
        EventBus.Instance.ClearEvents();
    }

    public void ShowdownHandler()
    {
        InitLane(true);
        InitLane(false);
        EventBus.Instance.RefreshBattleLoop += BattleRefreshHandler;
        DoBattle();
        //currLane.RemoveCardAtIndex(2);
        //actionQueue.Add(new Callable(this, MethodName.UpdateLanes));
    }

    public void BattleRefreshHandler()
    {
        UpdateLane(true);
        UpdateLane(false);
        FlipTurn();
        if (playerLane.CardCount <= 0)
        {
            // playerlost
            // EndRound();
            OnShowdownEndPlayerWin?.Invoke(false);
        }
        else if (enemyLane.CardCount <= 0)
        {
            // player won 
            // EndRound();
            OnShowdownEndPlayerWin?.Invoke(true);
        }
        else
            DoBattle();
    }

    private void DoBattle()
    {
        var currLane = battleState.currentTurn == Turn.Player ? playerLane : enemyLane;
        foreach (var eff in currLane.GetCardAtIndex(0).OnUse)
        {
            //GD.Print("called in onuse!");

            var effectParam = new EffectParam();
            effectParam.Initialize(battleState, currLane.GetCardAtIndex(0));

            eff.OnUse(effectParam);
            eff.OnEnqueue(effectParam);
            //GD.Print("waiting?");
        }

    }

    private void BuildQueue()
    {

    }

    private void FlipTurn()
    {
        if (battleState.currentTurn == Turn.Player)
            battleState.currentTurn = Turn.Enemy;
        else if (battleState.currentTurn == Turn.Enemy)
            battleState.currentTurn = Turn.Player;
    }

    private void Action(Callable call, bool isAnimation)
    {

    }

    private void InitLane(bool player)
    {
        var targetLane = player ? playerLane : enemyLane;
        targetLane.SetCombat();
        for (int i = 0; i < targetLane.CardCount; i++)
        {
            foreach (var effect in targetLane.GetCardAtIndex(i).Passives)
            {
                var effectParam = new EffectParam();
                effectParam.Initialize(battleState, targetLane.GetCardAtIndex(i));
                effect.Initialize(effectParam);
            }
        }
    }

    private async void UpdateLane(bool player)
    {
        List<CardData> toRemove = [];
        var targetLane = player ? playerLane : enemyLane;
        for (int i = 0; i < targetLane.CardCount; i++)
        {
            //GD.Print($"Turn is player: {player} idx: {i} HP is {targetLane.GetCardAtIndex(i).HP}");
            if (targetLane.GetCardAtIndex(i).HP <= 0)
            {
                GD.Print($"got a to remove at idx {i}");
                toRemove.Add(targetLane.GetCardAtIndex(i));
            }

            targetLane.GetBaseAtIndex(i).Visual.UpdateLabels();
        }
        //GD.Print(DeckManager.Instance.EnemyHand);
        foreach (var remCard in toRemove)
        {
            await targetLane.RemoveCard(remCard);
            DeckManager.Instance.Discard(remCard, player);
        }
    }

    private void Hide(Button button)
    {
        if (button == null) return;
        button.Visible = false;
        button.Disabled = true;
    }

    private void Show(Button button)
    {
        if (button == null) return;
        button.Visible = true;
        button.Disabled = false;
    }
}
