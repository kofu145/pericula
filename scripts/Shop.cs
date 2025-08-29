using Godot;
using System;
using System.Collections;
using System.Collections.Generic;


public partial class Shop : Control
{
    [Export] HBoxContainer shopChoices;
    [Export] PackedScene shopCardScene;
    [Export] int choicesAvailable = 5;
    const int REROLL_COST = 2;

    public override void _Ready()
    {
        base._Ready();
        Initialize();
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
