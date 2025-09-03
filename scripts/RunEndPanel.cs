using Godot;
using System;
using System.Collections.Generic;

public partial class RunEndPanel : CanvasLayer
{
    [Export] private RichTextLabel header;
    [Export] private RichTextLabel description;

    public void InitializeWin()
    {
        header.BbcodeEnabled = true;
        header.Text = "[wave]You Won![/wave]";

        FormatDescription();
    }

    public void InitializeLoss()
    {
        header.Text = $"You Lost to {StageManager.Instance.GetCurrentEnemy().DisplayName}";

        FormatDescription();
    }

    private void FormatDescription()
    {
        var rm = RunEndManager.Instance;
        var deckManips = Lookup.GetMostUsedManip();
        string deckManipString = "";

        // if (deckManips.Count > 1) deckManipString = FormatDeckManipString(deckManips);

        description.Text = $"You earned a total of: {ChipManager.Instance.ChipsEarned} Chips\n"
        + $"You spent a total of: {ChipManager.Instance.ChipsUsed} Chips\n"
        + $"You lost {rm.BetsLost} bets in total\n"
        + $"You Won {rm.BetsWon} bets in total\n"
        // + deckManipString
        ;
    }

    private string FormatDeckManipString(List<DeckManipData> deckManips)
    {
        string returnString = "";
        foreach (var manip in deckManips)
        {
            returnString += $"You used {manip.DisplayName} {manip.TimesUsed} times\n";
        }

        return returnString;
    }

    private void ReturnToTitleScreen()
    {
        SceneManager.ChangeSceneToFile("TitleScreen");
        Visible = false;
        RunEndManager.Instance.Reset();
        // reset usage for deckManips
        foreach (var manip in Lookup.GetDeckManipLibrary())
            if (manip is DeckManipData manipData) manipData.Reset();
    }
}
