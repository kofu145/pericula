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
    private List<CodexItemData> codexDataList = new();
    private int currentPage = 0;
    private int maxPage = 0;
    private List<Node> spawned = new();


    public override void _Ready()
    {
        SwitchToCardPage();

        prevButton.Pressed += SwitchToPreviousPage;
        nextButton.Pressed += SwitchToNextPage;
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

    private void SwitchToDeckManipPage()
    {
        codexDataList = Lookup.GetDeckManipLibrary();
        InitializePage();
    }

    private void SwitchToCardPage()
    {
        codexDataList = Lookup.GetCardLibrary();
        InitializePage();
    }

    private void PopulatePage()
    {
        ClearPage();

        int startIndex = CARDS_PER_PAGE * currentPage;
        for (int i = 0; i < CARDS_PER_PAGE; i++)
        {
            var card = startIndex + i < codexDataList.Count ? codexDataList[startIndex + i] : null;
            // card.Initialize()
            bool spawnLeftGrid = i < CARDS_PER_PAGE / 2;
            Spawn(card, spawnLeftGrid);
        }
    }


    // helper functions
    private void InitializePage()
    {
        currentPage = 0;
        maxPage = (int)Math.Ceiling((double)codexDataList.Count / CARDS_PER_PAGE) - 1;

        PopulatePage();
        UpdateButtons();
    }

    private void ClearPage()
    {
        foreach (var spawn in spawned) spawn.QueueFree();
        spawned.Clear();
    }
    private void Spawn(CodexItemData data, bool spawnLeftGrid)
    {
        var slot = codexSlot.Instantiate<CodexEntry>();

        if (spawnLeftGrid) leftGrid.AddChild(slot);
        else rightGrid.AddChild(slot);

        if (data != null) slot.Initialize(data);

        spawned.Add(slot);
    }

    private void UpdateButtons()
    {
        prevButton.Visible = currentPage != 0;
        nextButton.Visible = currentPage != maxPage;
    }
}
