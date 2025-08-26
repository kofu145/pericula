using Godot;
using System;
using System.Collections.Generic;

public partial class CardLane : Node
{
	// scene refs
	[Export] private PackedScene SlotScene;
	[Export] private PackedScene CardScene;
	[Export] private PackedScene CardVisual;

	[Export] private HBoxContainer lane;

	// Permissions
	[Export] private LaneSide side;

	// runtime refs
	private List<CardSlot> _slots = new();
	private List<CardBase> _cards = new();
	private List<CardVisual> _visuals = new();

	public void BindSide(LaneSide s) => side = s;

	public void EndRound()
	{
		ClearLane();
	}


	public void SpawnCard(CardData data)
	{
		var slot = SlotScene.Instantiate<CardSlot>();
		lane.AddChild(slot);
		_slots.Add(slot);

		var cardBase = CardScene.Instantiate<CardBase>();
		slot.AddChild(cardBase);
		_cards.Add(cardBase);

		cardBase.Initialize(data);

		if (side == LaneSide.Player)
		{
			cardBase.OnStartDrag += BeginDrag;
			cardBase.OnDragging += Drag;
			cardBase.OnEndDrag += EndDrag;
		}
		else
		{
			cardBase.EnableDefaultDrag = false;
			cardBase.EnableHoverScale = false;
		}
	}

	private void ClearLane()
	{
		foreach (var c in _cards) c.QueueFree();
		foreach (var s in _slots) s.QueueFree();
		foreach (var v in _visuals) v.QueueFree();
		_cards.Clear();
		_slots.Clear();
		_visuals.Clear();
	}

	public void RevealAtIndex(int index) => _visuals[index].ShowInfo();


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
		MoveInList(_visuals, currentIndex, desired);

	}

	private void EndDrag(CardBase c)
	{
		int end = IndexOf(c);
		var targetSlot = _slots[end];
		c.Reparent(targetSlot);

		c.TopLevel = false;
		c.ZIndex = 0;
	}

	private int IndexOf(CardBase c) => _cards.FindIndex(cr => cr == c);

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
				w.Reparent(_slots[i - 1]);
			}
		}
		else
		{
			// dragging left: shift [from-1..to] right by 1
			for (int i = from - 1; i >= to; i--)
			{
				var w = _cards[i];
				w.Reparent(_slots[i + 1]);
			}
		}
	}

}
