using Godot;
using System;

public partial class ChipManager : Node
{
    [Export] private int balance;
    public int Balance => balance;
    public static ChipManager Instance { get; private set; }
    public Action<int> OnChipsChanged;

    // run info of chips
    public int ChipsEarned { get; private set; }
    public int ChipsUsed { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
    // encapsulating the below is not strictly necessary, but 
    // may come in handy when potentially setting through events
    // (for instance, tracking whenever a player is deducted) for future

    public void StartNewRun(int startingChips)
    {
        balance = startingChips;
        OnChipsChanged?.Invoke(startingChips);
        ChipsEarned = 0;
        ChipsUsed = 0;
    }

    /// <summary>
    /// Method to deduct from total balance. 
    /// Returns true if balance is sufficient
    /// enough to deduct from, false if not.
    /// </summary>
    public bool Deduct(int amount)
    {
        bool valid = balance >= amount;
        balance = valid ? balance - amount : balance;
        if (valid)
        {
            OnChipsChanged?.Invoke(balance);
            ChipsUsed += amount;
        }

        SoundManager.PlaySE("chipsdrop");
        return valid;
    }

    /// <summary>
    /// Adds an int amount to balance.
    /// </summary>
    public void AddChips(int amount)
    {
        balance += amount;
        ChipsEarned += amount;
        OnChipsChanged?.Invoke(balance);
    }

    /// <summary>
    /// Method to deduct from total balance. 
    /// With this method, the balance can be negative.
    /// </summary>
    public void BorrowChips(int amount)
    {
        balance -= amount;
        ChipsUsed += amount;
        OnChipsChanged?.Invoke(balance);
    }
}
