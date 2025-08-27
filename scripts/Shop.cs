using Godot;
using System;
using System.Collections;
using System.Collections.Generic;


public partial class Shop : Control
{
    [Export] HBoxContainer shopChoices;
    [Export] PackedScene shopCardScene;
    [Export] int choicesAvailable = 5;
    [Export] RichTextLabel chipCount;

    ChipManager Chips;

    const int REROLL_COST = 2;

    public override void _Ready()
    {
        Chips = GetNode<ChipManager>("/root/GlobalManager/ChipManager");
        Chips.AddChips(100);
        base._Ready();
        Reroll();
    }

    public void Initialize()
    {
        int _upgradeID;
        for (int i = 0; i < choicesAvailable; i++)
        {
            _upgradeID = GD.RandRange(0, 10); // TODO: update w/ ids in-game
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
        if (Chips.Balance < REROLL_COST)
        {
            GD.Print("Not enough chips to reroll!");
            return;
        }
        else
        {
            Chips.Deduct(REROLL_COST);
            UpdateChipCount();
            Clear();
            Initialize();
        }
    }

    public void UpdateChipCount()
    {
        chipCount.Text = Chips.Balance.ToString();
    }

    public void EndShopPhase()
    {
        SceneManager.ChangeSceneToFile("scenes/Combat.tscn");
    }
}
