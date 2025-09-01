using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Lookup : Node
{
    public static Lookup Instance { get; private set; }

    const string CARD_DATA_PATH = "res://resources/cardData/";
    const string DECK_MANIP_PATH = "res://resources/deckManipData/";

    [Export]
    public Godot.Collections.Array<CardData> table = new();
    [Export]
    public Godot.Collections.Array<DeckManipData> deckmanipData = new();

    private static Godot.Collections.Array<CardData> lookupList = new();
    private static Godot.Collections.Array<DeckManipData> deckManipList = new();

    public override void _Ready()
    {
        Instance = this;
        /*
        foreach (string filepath in DirAccess.GetFilesAt(CARD_DATA_PATH))
        {
            if (filepath.EndsWith(".tres"))
            {
                var cardData = GD.Load<CardData>(CARD_DATA_PATH + filepath);
                lookupList.Add(cardData);
            }
        }*/
        foreach (var card in table)
        {
            lookupList.Add(card);
        }

        foreach (var card in deckmanipData)
        {
            deckManipList.Add(card);
        }
    }

    public static CardData GetCardByID(int id)
    {
        foreach (var card in lookupList)
        {
            if (card.id == id)
            {
                card.Initialize();
                return card;
            }
        }

        GD.PrintErr("Couldn't find card with ID: " + id);
        return null;
    }

    public static Godot.Collections.Array<CardData> GetCardsByRarity(RarityType rarity)
    {
        Godot.Collections.Array<CardData> result = new();
        foreach (var card in lookupList)
        {
            if (card.Rarity.RarityType == rarity)
            {
                result.Add(card);
            }
        }

        return result;
    }

    public static DeckManipData GetDeckManipByID(int id)
    {

        foreach (var card in deckManipList)
        {
            if (card.id == id)
                return card;
        }
        foreach (string filePath in DirAccess.GetFilesAt(DECK_MANIP_PATH))
        {
            if (filePath.EndsWith(".tres"))
            {
                var manipData = GD.Load<DeckManipData>(DECK_MANIP_PATH + filePath);
                if (manipData != null && manipData.id == id)
                {
                    return manipData;
                }
            }
        }

        GD.PrintErr("Couldn't find deck manip with ID: " + id);
        return null;
    }

    public static List<CardData> GetCardList()
    {
        List<CardData> cardList = [];
        foreach (string filePath in DirAccess.GetFilesAt(CARD_DATA_PATH))
        {
            if (filePath.EndsWith(".tres"))
            {
                var cardData = GD.Load<CardData>(CARD_DATA_PATH + filePath);
                if (cardData != null)
                {
                    cardList.Add(cardData);
                }
            }
        }

        return cardList;
    }

    public static List<CodexItemData> GetCardLibrary()
    {
        return lookupList
            .Where(c => c != null)
            .OrderBy(c => c.GetSortKey())
            .Cast<CodexItemData>()
            .ToList();

    }

    public static List<CodexItemData> GetDeckManipLibrary()
    {
        return deckManipList
            .Where(c => c != null)
            .OrderBy(c => c.GetSortKey())
            .Cast<CodexItemData>()
            .ToList();
    }

}
