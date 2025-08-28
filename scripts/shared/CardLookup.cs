using Godot;
using System;
using System.Collections.Generic;

public partial class CardLookup : Node
{
	public static CardLookup Instance { get; private set; }

	const string CARD_DATA_PATH = "res://resources/cardData/";
	const int TOTAL_CARD_COUNT = 4;

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

	public static List<CardData> GetCardList()
	{
		List < CardData > cardList = [];
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
