using Godot;
using System;

public partial class CardLookup : Node
{
	public static CardLookup Instance { get; private set; }

	const string CARD_DATA_PATH = "res://resources/cardData/";

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
				GD.Print(filePath);
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
}
