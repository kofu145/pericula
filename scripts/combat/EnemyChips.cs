using System;

public class EnemyChips
{
	private int balance = 0;
	public int Balance => balance;
	public Action<int> OnChipsChanged;

	public EnemyChips(int amount)
	{
		balance += amount;
		OnChipsChanged?.Invoke(balance);
	}
	
	public bool Deduct(int amount)
	{
		bool valid = balance >= amount;
		balance = valid ? balance - amount : balance;
		if (valid) OnChipsChanged?.Invoke(balance);
		return valid;
	}

	public void AddChips(int amount)
	{
		balance += amount;
		OnChipsChanged?.Invoke(balance);
	}
}
