using Godot;
using System;
using System.Collections;

public partial class ShopManager : Node
{
    public static ShopManager Instance;
    [Export] private RunConfig config;

    // runtime refs of run info
    private int removalsUsed = 0;

    public override void _Ready()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }
    }

    public void StartNewRun()
    {
        removalsUsed = 0;
    }

    public int GetUpgradeCost() => config.baseUpgradeCost;
    public int GetRemovalCost() => config.baseRemovalCost;
    public int GetCreateRandomCardCost() => config.baseCreateRandomCardCost;
    public int GetDuplicateCost() => config.baseDuplicateCost;

    public int GetManipPrice(int id)
    {
        switch (id)
        {
            case 1:
                return config.baseRemovalCost;
            case 2:
                return config.baseDuplicateCost;
            case 3:
                return config.baseUpgradeCost;
            case 4:
                return config.baseCreateRandomCardCost;
            default:
                return 0;
        }
    }
    public int GetCardPrice(CardData data) => data.Rarity.ShopCost;
    public int GetCardPrice(Rarity rarity) => rarity.ShopCost;

    public bool TryRemoveCard(int playerChips)
    {
        if (playerChips >= GetRemovalCost())
        {
            // TODO: Remove card here from the DeckManager.Instance
            removalsUsed++;
            return true;
        }
        return false;
    }

}
