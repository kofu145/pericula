using Godot;
using System;

public partial class CodexEntry : TextureRect
{
    [Export] private PackedScene cardScene;
    [Export] private PackedScene deckManipScene;

    private Node node;

    public void Initialize(CodexItemData data)
    {
        if (data == null) return;
        if (data is CardData cardData)
        {
            var cardBase = cardScene.Instantiate<ShopCard>();
            cardData.Initialize();
            cardBase.CodexInitialize(cardData.id);
            AddChild(cardBase);
            node = cardBase;
        }
        else if (data is DeckManipData deckManipData)
        {
            var deckManipCard = deckManipScene.Instantiate<DeckManipCard>();
            deckManipCard.CodexInitialize(deckManipData.id);
            AddChild(deckManipCard);
            node = deckManipCard;
        }
    }

    public void Clear()
    {
        if (node != null) {node.QueueFree();node = null; }
    }
}
