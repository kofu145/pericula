using Godot;
using System;
using System.Collections.Generic;

public partial class Codex : Control
{
    // scenes
    // [Export] private PackedScene codexSlot;

    // UI refs
    [Export] private Label headerLabel;
    [Export] private Label pageCount;
    [Export] private Button prevButton;
    [Export] private Button nextButton;
    [Export] private Button returnHomeButton;
    // button refs for the tabs
    [Export] private Button cardTabButton;
    [Export] private Button incantationTabButton;

    // scene refs
    [Export] private GridContainer leftGrid;
    [Export] private GridContainer rightGrid;

    // runtime refs
    private List<CodexEntry> codexSlots = new();
    private List<CodexItemData> codexDataList = new();
    private int currentPage = 0;
    private int maxPage = 0;
    private int slotCount;
    private ButtonGroup tabGroup = new();


    public override void _Ready()
    {
        for (int i = 0; i < leftGrid.GetChildCount(); i++)
        {
            if (leftGrid.GetChild(i) is CodexEntry slot)
            {
                codexSlots.Add(slot);
            }
        }
        for (int i = 0; i < rightGrid.GetChildCount(); i++)
        {
            if (rightGrid.GetChild(i) is CodexEntry slot)
            {
                codexSlots.Add(slot);
            }
        }
        slotCount = codexSlots.Count;

        SwitchToCardPage();

        prevButton.Pressed += SwitchToPreviousPage;
        nextButton.Pressed += SwitchToNextPage;
        returnHomeButton.Pressed += ReturnToTitleScreen;

        // tab handlers
        cardTabButton.Pressed += SwitchToCardPage;
        incantationTabButton.Pressed += SwitchToDeckManipPage;

        cardTabButton.ButtonGroup = tabGroup;
        incantationTabButton.ButtonGroup = tabGroup;
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
        headerLabel.Text = "Incantations";
        codexDataList = Lookup.GetDeckManipLibrary();
        InitializePage();

        // disable itself, enable all others in group
        foreach (var button in tabGroup.GetButtons()) button.Disabled = false;
        incantationTabButton.Disabled = true;
    }

    private void SwitchToCardPage()
    {
        headerLabel.Text = "Cards";
        codexDataList = Lookup.GetCardLibrary();
        InitializePage();
        foreach (var button in tabGroup.GetButtons()) button.Disabled = false;
        cardTabButton.Disabled = true;
    }

    private void ReturnToTitleScreen()
    {
        SceneManager.ChangeSceneToFile("TitleScreen");
    }

    private void PopulatePage()
    {
        ClearPage();

        int startIndex = slotCount * currentPage;
        for (int i = 0; i < slotCount; i++)
        {
            int currentIndex = startIndex + i;
            var card = currentIndex < codexDataList.Count ? codexDataList[currentIndex] : null;

            codexSlots[i].Initialize(card);
        }
        pageCount.Text = $"{currentPage + 1} / {maxPage + 1}";
    }


    // helper functions
    private void InitializePage()
    {
        currentPage = 0;
        maxPage = (int)Math.Ceiling((double)codexDataList.Count / slotCount) - 1;

        PopulatePage();
        UpdateButtons();
    }

    private void ClearPage()
    {
        foreach (var slot in codexSlots) slot.Clear();
    }
    private void UpdateButtons()
    {
        prevButton.Visible = currentPage != 0;
        nextButton.Visible = currentPage != maxPage;
    }
}
