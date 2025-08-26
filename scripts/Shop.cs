using Godot;
using System;

public partial class Shop : Control
{
    [Export] VBoxContainer shopChoices;
    [Export] PackedScene shopCardScene;
    [Export] int choicesAvailable = 5;

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
            _upgradeID = GD.RandRange(0, 100); // TODO: update w/ ids in-game
            CreateOffer(_upgradeID);
        }
    }

    public void Clear()
    {
        foreach (ShopCard card in shopChoices.GetChildren())
        {
            card.RemoveFromShop();
        }
    }

    public void CreateOffer(int id)
    {
        ShopCard card = shopCardScene.Instantiate<ShopCard>();
        card.AssignUpgrade(id);

        shopChoices.AddChild(card);
    }

    public void Reroll()
    {
        Clear();
        Initialize();
    }
}
