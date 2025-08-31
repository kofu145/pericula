using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

public partial class CardLane : Node
{
    // scene refs
    // [Export] private PackedScene SlotScene;
    [Export] private PackedScene CardScene;

    [Export] private HBoxContainer lane;

    public int CardCount => _cards.Count;
    public LaneSide Side => side;

    // Permissions
    [Export] private LaneSide side = LaneSide.Player;

    // runtime refs
    private List<CardSlot> _slots = new();
    private List<CardBase> _cards = new();

    // ========================
    // public APIs
    // ========================
    public int SlotIndexOf(CardSlot slot) => _slots.IndexOf(slot);
    public int IndexOf(CardBase c) => _cards.FindIndex(x => x == c);

    private bool isFlopPhase = false;

    public override void _Ready()
    {
        for (int i = 0; i < lane.GetChildCount(); i++)
        {
            if (lane.GetChild(i) is CardSlot slot)
            {
                slot.OwnerLane = this;
                _slots.Add(slot);
            }
        }
    }

    public void EndRound()
    {
        ClearLane();
    }

    public void SpawnCard(CardData data, int index)
    {
        var cardBase = CardScene.Instantiate<CardBase>();
        _slots[index].AddChild(cardBase);
        _cards.Insert(index, cardBase);

        cardBase.Initialize(data);
        cardBase.IsPlayer = side == LaneSide.Player;
        if (side == LaneSide.Player)
        {
            SubscribeCard(cardBase);
        }
        else
        {
            cardBase.EnableDefaultDrag = false;
            cardBase.EnableHoverScale = false;
        }
    }

    public void PrintCards()
    {
        foreach (var card in _cards)
        {
            GD.Print(card.Data);
        }
    }

    public bool Contains(CardData card)
    {
        return _cards.Contains(GetCardBaseByData(card));
    }

    public void ToggleVisualLerp(bool value)
    {
        foreach (var card in _cards)
        {
            card.Visual.ToggleLerp(value);
        }
    }

    // honestly this method should just replace the one below but we would have to refactor a bunch of shit if did so
    public CardBase GetBaseAtIndex(int idx)
    {
        return _cards[idx];
    }

    public CardData GetCardAtIndex(int idx)
    {
        return _cards[idx].Data;
    }

    public CardBase GetCardBaseByData(CardData data)
    {
        CardBase target = null;
        foreach (var card in _cards)
        {
            if (card.Data == data)
                target = card;
        }

        return target;
    }

    public List<CardData> GetAllCardData()
    {
        List<CardData> returnList = new();

        foreach (var card in _cards)
        {
            returnList.Add(card.Data);
        }

        return returnList;
    }

    public void DisableInteraction()
    {
        foreach (var card in _cards)
        {
            card.EnableDefaultDrag = false;
            UnsubscribeCard(card);
        }
    }

    public void EnableInteraction()
    {
        foreach (var card in _cards)
        {
            card.EnableDefaultDrag = true;
            SubscribeCard(card);
        }
    }

    public async Task RemoveCardAtIndex(int idx, bool doReparent = true)
    {
        if (idx < 0 || idx >= _cards.Count)
        {
            GD.Print("invalid range for removing card, aborting");
            return;
        }
        //GD.Print(idx);
        var cardToRemove = _cards[idx];
        _cards.RemoveAt(idx);
        cardToRemove.QueueFree();


        if (doReparent)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                var pos = _cards[i].Visual.GlobalPosition;
                //GD.Print($"Moving from {pos} to {_slots[i].GlobalPosition}");
                _cards[i].Reparent(_slots[i], false);
                _cards[i].Visual.SetOffset(pos);
                _cards[i].Visual.GlobalPosition = pos;
                //GD.Print($"Moving from {pos} to {_slots[i].GlobalPosition}");
                _cards[i].Visual.ToggleLerp(true);
                await ToSignal(GetTree().CreateTimer(.05), Timer.SignalName.Timeout);
            }
        }

    }

    public async Task RemoveCard(CardData cardData, bool doReparent = true)
    {
        var target = GetCardBaseByData(cardData);

        int idx = IndexOf(target);
        await RemoveCardAtIndex(idx, doReparent);
    }



    // Utility to (un)wire a card's drag events to THIS lane's handlers.
    // Call these when a card changes lanes.
    public void SubscribeCard(CardBase card)
    {
        card.OnStartDrag += BeginDrag;
        card.OnDragging += Drag;
        card.OnEndDrag += EndDrag;
    }
    public void UnsubscribeCard(CardBase card)
    {
        card.OnStartDrag -= BeginDrag;
        card.OnDragging -= Drag;
        card.OnEndDrag -= EndDrag;
    }

    public void SetFlopPhase()
    {
        isFlopPhase = true;
        if (side == LaneSide.Player)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if ((i == 3 || i == 4) && _cards[i].Visual.isFaceUp)
                {
                    _cards[i].FlipCard();
                }
            }
        }
        else
        {
            foreach (var card in _cards)
            {
                if (card.Visual.isFaceUp)
                    card.FlipCard();
            }
        }
    }

    public void SetBetPhase()
    {
        isFlopPhase = false;
        DisableInteraction();
        for (int i = 0; i < _cards.Count; i++)
        {
            if (i < 3 && !_cards[i].Visual.isFaceUp)
            {
                _cards[i].FlipCard();
            }
        }
    }

    public void SetCombat()
    {
        DisableInteraction();
        isFlopPhase = false;
        foreach (var card in _cards)
        {
            if (!card.Visual.isFaceUp)
            {
                card.FlipCard();
            }
        }
    }

    // ==============================================
    // private helpers
    // ==============================================

    private void ClearLane()
    {
        DeckManager.Instance.ClearHand(side == LaneSide.Player);
        GD.Print("free");
        foreach (var c in _cards) c.QueueFree(); // don't want to free the children carddata, we use them elsewhere in deck

        _cards.Clear();
    }

    public void FlipAtIndex(int index) => _cards[index].FlipCard();

    private void BeginDrag(CardBase c)
    {
        int index = IndexOf(c);
        if (index < 0) return;

        var original = c.GlobalPosition;
        c.TopLevel = true;
        c.GlobalPosition = original;
        c.ZIndex = 10;
    }

    private void Drag(CardBase c)
    {
        int currentIndex = IndexOf(c);
        if (currentIndex < 0) return;

        int desired = ClosestSlotIndex(c.GlobalPosition.X);

        ShiftOthers(currentIndex, desired);

        MoveInList(_cards, currentIndex, desired);
        for (int i = 0; i < _cards.Count; i++)
        {
            SetBackState(_cards[i], i);
        }
    }

    private void EndDrag(CardBase c)
    {
        int end = IndexOf(c);
        var targetSlot = _slots[end];
        c.Reparent(targetSlot);

        c.TopLevel = false;
        c.ZIndex = 0;
    }

    private int ClosestSlotIndex(float globalX)
    {
        int best = 0;
        float min = float.MaxValue;
        for (int i = 0; i < _slots.Count; i++)
        {
            var centerX = _slots[i].GetGlobalRect().GetCenter().X;
            float d = Mathf.Abs(globalX - centerX);
            if (d < min) { min = d; best = i; }
        }
        return best;
    }

    private void MoveInList<T>(List<T> list, int from, int to)
    {
        if (from == to) return;
        var item = list[from];
        list.RemoveAt(from);
        list.Insert(to, item);
    }

    private void ShiftParents(int from, int to)
    {
        ShiftOthers(from, to);

        var dragged = _cards[from];
        dragged.Reparent(_slots[to]);
    }


    private void ShiftOthers(int from, int to)
    {
        if (from == to) return;

        if (to > from)
        {
            // dragging right: shift [from+1..to] left by 1
            for (int i = from + 1; i <= to; i++)
            {
                var w = _cards[i];
                if (w != null)
                    w.Reparent(_slots[i - 1]);

            }
        }
        else
        {
            // dragging left: shift [from-1..to] right by 1
            for (int i = from - 1; i >= to; i--)
            {
                var w = _cards[i];
                if (w != null)
                    w.Reparent(_slots[i + 1]);
            }
        }
    }

    private void SetBackState(CardBase card, int idxAt)
    {

        if (isFlopPhase && (idxAt == 3 || idxAt == 4) && card.Visual.isFaceUp)
        {
            card.FlipCard();
        }
        else if (idxAt < 3)
        {
            if (!card.Visual.isFaceUp)
            {
                card.FlipCard();
            }
        }
    }


}
