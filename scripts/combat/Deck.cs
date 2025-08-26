
using System;
using System.Collections.Generic;

public partial class Deck
{
	private readonly List<CardData> _draw = new();
	private readonly List<CardData> _discard = new();

	private readonly Random _rng = new();

	public int DrawCount => _draw.Count;
	public int DiscardCount => _discard.Count;

	public Deck(IEnumerable<CardData> cards)
	{
		_draw.AddRange(cards);
		Shuffle();
	}

	public List<CardData> Draw(int n)
	{
		var outList = new List<CardData>();

		for (int i = 0; i < n; i++)
		{
			if (_draw.Count == 0)
			{
				if (_discard.Count == 0) break;
				RecycleDiscardIntoDraw();
			}

			var lastIndex = _draw.Count - 1;
			var c = _draw[lastIndex];
			_draw.RemoveAt(lastIndex);
			outList.Add(c);
		}
		return outList;
	}

	public void Discard(CardData c) => _discard.Add(c);
	public void DiscardRange(IEnumerable<CardData> cards) => _discard.AddRange(cards);

	private void RecycleDiscardIntoDraw()
	{
		_draw.AddRange(_discard);
		_discard.Clear();
		Shuffle();
	}

	private void Shuffle()
	{
		for (int i = 0; i < _draw.Count; i++)
		{
			int j = _rng.Next(i + 1);
			(_draw[i], _draw[j]) = (_draw[j], _draw[i]);
		}
	}
}
