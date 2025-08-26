using Godot;
using System;
using System.Collections.Generic;

public partial class DeckListView : Control
{
	// Scene refs
	[Export] private PackedScene CardScene;
	[Export] private PackedScene CardVisualScene;

	// UI refs
	[Export] private ScrollContainer Scroll;
	[Export] private GridContainer Grid;
	[Export] private Control visualContainer;

	// Layout config
	[Export] private Vector2I CardSize = new(260, 360); // pixel size of each card cell
	[Export] private int HGap = 24;
	[Export] private int VGap = 24;

	// Dummy data for inspector testing
	[Export] private CardData inspectorCard;
	[Export] private int dummyCardCount = 10;

	// runtime refs
	private List<CardBase> _cards = new();
	private List<CardVisual> _visuals = new();


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Grid.AddThemeConstantOverride("h_separation", HGap);
		Grid.AddThemeConstantOverride("v_separation", VGap);

		Resized += UpdateColumns;
		if (Scroll != null) Scroll.Resized += UpdateColumns;

		CallDeferred(nameof(UpdateColumns));

		// TEMP: populate with dummy cards
		var dummyDeck = new List<CardData>();
		for (int i = 0; i < dummyCardCount; i++) dummyDeck.Add(inspectorCard);
		Populate(dummyDeck);
	}

	private void UpdateColumns()
	{
		// Width available to the grid (inside the ScrollContainer)
		float avail = Scroll?.Size.X ?? Size.X;

		// Each cell takes card width + horizontal gap (except the last column)
		float cell = CardSize.X + HGap;

		// Columns = how many cells fit; clamp to >= 1
		int cols = Mathf.Max(1, Mathf.FloorToInt((avail + HGap) / cell));
		Grid.Columns = cols;
	}

	public void Clear()
	{
		foreach (var c in _cards) c.QueueFree();
		foreach (var c in _visuals) c.QueueFree();
		_cards.Clear();
		_visuals.Clear();
	}

	/// <summary>
	/// Populates the deck view with the given list of cardDatas.
	/// </summary>
	/// <param name="deck"> list of cards to populate the view with</param>
	public void Populate(List<CardData> deck)
	{
		Clear();
		foreach (var data in deck) SpawnCard(data);
	}

	private void SpawnCard(CardData data)
	{
		var cardBase = CardScene.Instantiate<CardBase>();
		cardBase.EnableDefaultDrag = false;
		Grid.AddChild(cardBase);
		_cards.Add(cardBase);

		var cardVisual = CardVisualScene.Instantiate<CardVisual>();
		visualContainer.AddChild(cardVisual);
		_visuals.Add(cardVisual);

		cardVisual.Initialize(cardBase, data);
	}

	public void ToggleDeckView()
	{
		if (Visible)
		{
			CloseDeckView();
		}
		else
		{
			OpenDeckView();
		}

		Visible = !Visible;
	}

	private void OpenDeckView()
	{

	}

	private void CloseDeckView()
	{

	}
}
