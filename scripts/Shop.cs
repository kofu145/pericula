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

    // UI Ref
    [Export] private Button rerollButton;

    [Export] private bool testMode = false;


    // runtime
    private int currentRerollCost = 0;

    public override void _Ready()
    {
        base._Ready();
        Initialize();
        InitializeDeckManipOptions();

        ShopManager.Instance.Resetreroll();
        UpdateRerollCost();
        if (testMode) ChipManager.Instance.AddChips(100000);
    }

    public void Initialize()
    {
        int _upgradeID;
        for (int i = 0; i < choicesAvailable; i++)
        {
            var rarity = ShopManager.Instance.GenRarity();
            var target = Lookup.GetCardsByRarity(rarity);
            _upgradeID = target[DeckManager.Instance.RndGen.Next(target.Count)].id;


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
        card.Initialize(id);

        shopChoices.AddChild(card);
    }

    public void CreateDeckManipOffer(int id)
    {
        DeckManipCard card = manipScene.Instantiate<DeckManipCard>();
        card.Initialize(id);

        manipChoices.AddChild(card);
    }

    public void Reroll()
    {
        if (!ChipManager.Instance.Deduct(currentRerollCost))
        {
            GD.Print("Not enough chips to reroll!");
            SoundManager.PlaySE("fail");
            return;
        }
        else
        {
            SoundManager.PlaySE("click");
            ChipManager.Instance.Deduct(currentRerollCost);
            ShopManager.Instance.Reroll();
            UpdateRerollCost();
            Clear();
            Initialize();
        }
    }

    public void EndShopPhase()
    {
        SceneManager.ChangeSceneToFile("PreCombat");
        SoundManager.PlaySE("click");
    }

    private void UpdateRerollCost()
    {
        currentRerollCost = ShopManager.Instance.GetRerollCost();
        rerollButton.Text = $"Reroll ({currentRerollCost})";
    }
}
