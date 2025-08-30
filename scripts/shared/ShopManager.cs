using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class ShopManager : Node
{
    public static ShopManager Instance;
    [Export] private RunConfig config;
    [Export] Godot.Collections.Array<Rarity> rarities;

    // runtime refs of run info
    private int rerollsUsed = 0;

    public override void _Ready()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }
    }

    public void StartNewRun()
    {
        rerollsUsed = 0;
    }

    public void Resetreroll()
    {
        rerollsUsed = 0;
    }

    public int GetUpgradeCost() => config.baseUpgradeCost;
    public int GetRemovalCost() => config.baseRemovalCost;
    public int GetCreateRandomCardCost() => config.baseCreateRandomCardCost;
    public int GetDuplicateCost() => config.baseDuplicateCost;

    public int GetRerollCost() => config.rerollCost * (int)Math.Pow(config.rerollMultiplierPerUse, rerollsUsed);
    public void Reroll() { rerollsUsed++; }


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

    public RarityType GenRarity()
    {
        // 1-indexed
        int currentAnte = StageManager.Instance.CurrentAnte + 1;

        // Build candidate pool: only >0 weight and available at this ante
        var candidates = new List<(Rarity rarity, int weight)>();
        foreach (var r in rarities)
        {
            if (r.Weights <= 0) continue;

            if (r.RarityType == RarityType.Mythic && currentAnte < config.mythicAvailableAtAnte)
                continue;

            if (r.RarityType == RarityType.Legendary && currentAnte < config.LegendaryAvailableAtAnte)
                continue;

            candidates.Add((r, r.Weights));
        }

        foreach (var candidate in candidates) GD.Print($"{candidate.rarity} weight: {candidate.weight}");

        // Safety: no eligible rarities
        if (candidates.Count == 0)
        {
            GD.PushWarning("GenRarity: no eligible rarities at this ante; falling back to Common.");
            return RarityType.Common; // or whatever your safe default is
        }

        // Sum weights
        int totalWeight = 0;
        foreach (var c in candidates)
            totalWeight += c.weight;

        // Roll and pick
        int roll = DeckManager.Instance.RndGen.Next(totalWeight); // [0, totalWeight)
        int cumulative = 0;
        foreach (var c in candidates)
        {
            cumulative += c.weight;
            if (roll < cumulative)
                return c.rarity.RarityType;
        }

        // Fallback (should be unreachable)
        return candidates[candidates.Count - 1].rarity.RarityType;
    }

}
