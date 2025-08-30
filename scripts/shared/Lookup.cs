using Godot;
using System;
using System.Collections.Generic;

public partial class Lookup : Node
{
	public static Lookup Instance { get; private set; }

	const string CARD_DATA_PATH = "res://resources/cardData/";
	const string DECK_MANIP_PATH = "res://resources/deckManipData/";

	public override void _Ready()
	{
		Instance = this;
	}

	public static CardData GetCardByID(int id)
	{
		foreach (string filePath in DirAccess.GetFilesAt(CARD_DATA_PATH))
		{
			if (filePath.EndsWith(".tres"))
			{
				var cardData = GD.Load<CardData>(CARD_DATA_PATH + filePath);
				if (cardData != null && cardData.id == id)
				{
					return cardData;
				}
			}
		}

		GD.PrintErr("Couldn't find card with ID: " + id);
		return null;
	}
	
	public static DeckManipData GetDeckManipByID(int id)
	{
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
}
