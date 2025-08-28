using Godot;
using System;

public partial class ChipManager : Node
{
	[Export] private int balance;
	public int Balance => balance;
	public static ChipManager Instance { get; private set; }
	public Action<int> OnChipsChanged;

	public override void _Ready()
	{
		Instance = this;
	}
	// encapsulating the below is not strictly necessary, but 
	// may come in handy when potentially setting through events
	// (for instance, tracking whenever a player is deducted) for future

	/// <summary>
	/// Method to deduct from total balance. 
	/// Returns true if balance is sufficient
	/// enough to deduct from, false if not.
	/// </summary>
	public bool Deduct(int amount)
	{
		bool valid = balance >= amount;
		balance = valid ? balance - amount : balance;
		if (valid) OnChipsChanged?.Invoke(balance);
		return valid;
	}

	/// <summary>
	/// Adds an int amount to balance.
	/// </summary>
	public void AddChips(int amount)
	{
		balance += amount;
		OnChipsChanged?.Invoke(balance);
	}
}
