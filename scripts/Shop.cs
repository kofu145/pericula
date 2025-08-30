using Godot;
using System;
using System.Collections;
using System.Collections.Generic;


public partial class Shop : Control
{
    [Export] HBoxContainer shopChoices;
    [Export] HBoxContainer manipChoices;
    [Export] PackedScene shopCardScene;
    [Export] PackedScene manipScene;
    [Export] int choicesAvailable = 5;
    [Export] int deckManipAvailable = 2;
    const int REROLL_COST = 2;

    public override void _Ready()
    {
        base._Ready();
        Initialize();
        InitializeDeckManipOptions();
    }

    public void Initialize()
    {
        int _upgradeID;
        for (int i = 0; i < choicesAvailable; i++)
        {
            _upgradeID = GD.RandRange(1, 4); // TODO: update w/ ids in-game
            CreateOffer(_upgradeID);
        }
    }

    public void InitializeDeckManipOptions()
    {
        int _deckManipID;
        for (int i = 0; i < deckManipAvailable; i++)
        {
            _deckManipID = GD.RandRange(1, 4); // TODO: update w/ ids in-game
            CreateDeckManipOffer(_deckManipID);
        }
    }

    public void Clear()
    {
        foreach (ShopCard card in shopChoices.GetChildren())
        {
            card.QueueFree();
        }
    }

    public void CreateOffer(int id)
    {
        ShopCard card = shopCardScene.Instantiate<ShopCard>();
        card.Initialize(id, this);

        shopChoices.AddChild(card);
    }

    public void CreateDeckManipOffer(int id)
    {
        DeckManipCard card = manipScene.Instantiate<DeckManipCard>();
        card.Initialize(id, this);

        manipChoices.AddChild(card);
    }

    public void Reroll()
    {
        if (ChipManager.Instance.Balance < REROLL_COST)
        {
            GD.Print("Not enough chips to reroll!");
            return;
        }
        else
        {
            ChipManager.Instance.Deduct(REROLL_COST);
            Clear();
            Initialize();
        }
    }

    public void EndShopPhase()
    {
        SceneManager.ChangeSceneToFile("PreCombat");
    }
}
