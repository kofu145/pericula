using Godot;
using System;
using System.Collections.Generic;

public partial class Codex : Control
{
    // scenes
    [Export] private PackedScene codexSlot;

    // UI refs
    [Export] private Button prevButton;
    [Export] private Button nextButton;

    // scene refs
    [Export] private GridContainer leftGrid;
    [Export] private GridContainer rightGrid;

    // consts
    const int CARDS_PER_PAGE = 18;

    // runtime refs
    private List<CardData> cardList = new();
    private int currentPage = 0;
    private int maxPage = 0;
    private List<Node> spawned = new();


    public override void _Ready()
    {
        Initialize();

        prevButton.Pressed += SwitchToPreviousPage;
        nextButton.Pressed += SwitchToNextPage;
    }


    private void Initialize()
    {
        cardList = Lookup.GetCardLibrary();
        maxPage = (int)Math.Ceiling((double)cardList.Count / CARDS_PER_PAGE) - 1;

        PopulatePage();
        UpdateButtons();
    }

    // button handlers
    private void SwitchToPreviousPage()
    {
        currentPage--;
        UpdateButtons();
        PopulatePage();
    }

    private void SwitchToNextPage()
    {
        currentPage++;
        UpdateButtons();
        PopulatePage();
    }

    private void PopulatePage()
    {
        ClearPage();

        int startIndex = CARDS_PER_PAGE * currentPage;
        for (int i = 0; i < CARDS_PER_PAGE; i++)
        {
            var card = cardList[startIndex + i];
            // card.Initialize()
            bool spawnLeftGrid = (i < CARDS_PER_PAGE / 2);
            Spawn(card, spawnLeftGrid);
        }
    }


    // helper functions
    private void ClearPage()
    {
        foreach (var spawn in spawned) spawn.QueueFree();
        spawned.Clear();
    }
    private void Spawn(CardData data, bool spawnLeftGrid)
    {
        var slot = codexSlot.Instantiate<CodexEntry>();
        slot.Initialize(data);

        if (spawnLeftGrid) leftGrid.AddChild(slot);
        else rightGrid.AddChild(slot);

        spawned.Add(slot);
    }

    private void UpdateButtons()
    {
        prevButton.Visible = currentPage != 0;
        nextButton.Visible = currentPage != maxPage;
    }
}
