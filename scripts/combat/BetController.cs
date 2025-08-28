using Godot;
using System;
using System.Collections.Generic;

public enum Turn { Player, Enemy, None }
public enum BetAction { None, Check, Raise, Call, Fold, CallAndRaise, AllIn }

public partial class BetController : Node
{
	// UI refs
	[Export] private Button raiseButton;
	[Export] private Button callButton;
	[Export] private Button checkButton;
	[Export] private Button foldButton;
	[Export] private Button allInButton;

	[Export] private Button raise1xButton;
	[Export] private Button raise2xButton;
	[Export] private Button raise5xButton;

	// Config
	private int minimumBuyIn;

	// public events
	public Action<BetAction, int, int> OnEnemyAction; // parameters: (action, toCallAmount, raiseAmount)
	public Action<bool> OnBetPhaseEnd;
	public Action<int> OnChipsChanged; // parameter: (newChipAmount)

	// states
	private Turn turn = Turn.None;
	private BetAction lastEnemyAction = BetAction.None;
	private BetAction lastPlayerAction = BetAction.None;

	// current bet
	private int currentBet = 0;     // highest amount put in by either side
	private bool betOpen = false;

	// each side's total bet this round
	private int playerPut = 0;
	private int enemyPut = 0;
	private int pot = 0;            // total for both sides

	private EnemyChips enemyChips;

	private bool choosingRaiseAmount = false;

	private bool endedWithFold = false;
	private bool foldedByPlayer = false;

	private ChipManager playerChips;


	// temp implementation
	private RandomNumberGenerator rng;

	// ====================================
	private int PlayerBalance => playerChips.Balance;                               // player's current chip balance
	private int ToCall => Math.Max(0, currentBet - playerPut);                      // the amount to call the enemy's bet
	private int AffordableRaise => Math.Max(0, PlayerBalance - ToCall);             // the remaining balance the player has after calling
	private bool CanCall => ToCall > 0 && PlayerBalance >= ToCall;                  // player has enough chips to call the enemy's bet
	private bool CanOpenRaise => !betOpen && PlayerBalance >= minimumBuyIn && EnemyBalance >= minimumBuyIn;         // player has enough chips to raise (at least minimumBuyIn) 
	private bool CanRaiseOverCall => betOpen && AffordableRaise >= minimumBuyIn;    // player has enough chips to call the enemy's bet and raise (at least minimumBuyIn) 
	private int EnemyBalance => enemyChips.Balance;
	// ====================================


	public override void _Ready()
	{
		playerChips = ChipManager.Instance;

		// wire button handlers
		if (raiseButton != null) raiseButton.Pressed += OnClickPlayerRaise;
		if (callButton != null) callButton.Pressed += OnClickPlayerCall;
		if (checkButton != null) checkButton.Pressed += OnClickPlayerCheck;
		if (foldButton != null) foldButton.Pressed += OnClickPlayerFold;
		if (allInButton != null) allInButton.Pressed += OnClickPlayerAllIn;
		if (raise1xButton != null) raise1xButton.Pressed += () => OnClickPlayerRaiseAmount(1);
		if (raise2xButton != null) raise2xButton.Pressed += () => OnClickPlayerRaiseAmount(2);
		if (raise5xButton != null) raise5xButton.Pressed += () => OnClickPlayerRaiseAmount(5);

		HideAll();
		UpdatePotLabel();
	}
	public void BeginPhase(int minBuyIn, EnemyChips enemyChips, int startingPot)
	{
		// temp implementation of rng
		rng = new();
		rng.Randomize();

		minimumBuyIn = minBuyIn;
		this.enemyChips = enemyChips;
		pot = startingPot;

		if (raise1xButton != null) raise1xButton.Text = $"$ {minimumBuyIn}";
		if (raise2xButton != null) raise2xButton.Text = $"$ {minimumBuyIn * 2}";
		if (raise5xButton != null) raise5xButton.Text = $"$ {minimumBuyIn * 5}";

		// reset states for new bet
		turn = Turn.Player;
		lastEnemyAction = BetAction.None;
		lastPlayerAction = BetAction.None;

		playerPut = 0;
		enemyPut = 0;
		currentBet = 0;
		betOpen = false;

		endedWithFold = false;
		foldedByPlayer = false;

		choosingRaiseAmount = false;

		HideAll();
		Show(checkButton);
		Show(raiseButton);
		UpdatePotLabel();
	}


	// ============ Player Handlers =============
	private void OnClickPlayerRaise()
	{
		if (turn != Turn.Player) return;

		// guard: can't raise if not enough balance
		if ((!betOpen && PlayerBalance < minimumBuyIn) || (betOpen && AffordableRaise < minimumBuyIn))
			return;

		choosingRaiseAmount = true;
		UpdateButtons();
	}
	private void OnClickPlayerRaiseAmount(int multiplier)
	{
		if (turn != Turn.Player) return;

		int raiseAmount = minimumBuyIn * Math.Max(1, multiplier);
		int maxRaise = AffordableRaise;

		if (maxRaise <= 0)
		{
			OnClickPlayerAllIn();
			return;
		}

		raiseAmount = Math.Min(raiseAmount, maxRaise);  // clamp to max affordable
		ApplyPlayerRaise(raiseAmount);
	}

	private void ApplyPlayerRaise(int raiseAmount)
	{
		int amountToCall = ToCall;
		int balance = PlayerBalance;

		if (balance <= 0) return;
		if (balance < amountToCall)
		{
			// all in remaining
			int spendAll = balance;
			if (!playerChips.Deduct(spendAll)) return;
			playerPut += spendAll;
			pot += spendAll;
			lastPlayerAction = BetAction.AllIn; // all in to call
			EndPhase();
			return;
		}

		int maxRaise = Math.Max(0, balance - amountToCall);
		int finalRaise = Math.Min(Math.Max(0, raiseAmount), maxRaise);

		int spend = amountToCall + finalRaise;
		if (spend <= 0) return;

		if (!playerChips.Deduct(spend)) return;
		playerPut += spend;
		pot += spend;

		currentBet = Math.Max(playerPut, enemyPut);
		betOpen = true;
		lastPlayerAction = finalRaise > 0 ? BetAction.Raise : BetAction.Call;

		choosingRaiseAmount = false;
		turn = Turn.Enemy;

		UpdateButtons();
		UpdatePotLabel();
		EnemyAct();
	}

	private void OnClickPlayerCall()
	{
		if (turn != Turn.Player) return;

		int amountToCall = ToCall;
		if (amountToCall <= 0) return;

		int spend = Math.Min(amountToCall, PlayerBalance);
		if (spend <= 0) return;

		if (!playerChips.Deduct(spend)) return;

		playerPut += spend;
		pot += spend;

		lastPlayerAction = BetAction.Call;
		EndPhase();
	}

	private void OnClickPlayerCheck()
	{
		if (turn != Turn.Player) return;

		if (betOpen)
		{
			UpdateButtons();
			return;
		}

		lastPlayerAction = BetAction.Check;

		// both entities checked, end phase
		if (lastEnemyAction == BetAction.Check)
		{
			EndPhase();
			return;
		}

		turn = Turn.Enemy;
		UpdateButtons();
		UpdatePotLabel();
		EnemyAct();
	}

	private void OnClickPlayerFold()
	{
		if (turn != Turn.Player) return;

		lastPlayerAction = BetAction.Fold;
		endedWithFold = true;
		foldedByPlayer = true;
		EndPhase();
	}

	private void OnClickPlayerAllIn()
	{
		if (turn != Turn.Player) return;

		int balance = PlayerBalance;
		if (balance <= 0) return;

		balance = Math.Min(balance, EnemyBalance);  

		if (!playerChips.Deduct(balance)) return;
		pot += balance;
		currentBet = Math.Max(playerPut + balance, enemyPut);
		playerPut += balance;


		lastPlayerAction = BetAction.AllIn;

		turn = Turn.Enemy;
		UpdateButtons();
		UpdatePotLabel();
		EnemyAct();
	}

	// ============ Enemy Logic (Temporary) =============
	private void EnemyAct()
	{
		if (turn != Turn.Enemy) return;
		GD.Print("Player last action: " + lastPlayerAction);

		if (lastPlayerAction == BetAction.AllIn)
		{
			// player went all in, enemy must Call or Fold
			// TODO: Replace with AI logic and not random logic
			var choice = rng.RandiRange(0, 1); // 0 = call, 1 = fold

			if (choice == 0)   // call
			{
				int amountToCall = Math.Max(0, currentBet - enemyPut);

				if (amountToCall >= EnemyBalance)
				{
					// all in
					amountToCall = EnemyBalance;
					lastEnemyAction = BetAction.AllIn;
					OnEnemyAction?.Invoke(BetAction.AllIn, amountToCall, 0);
				}
				else
				{
					// regular call
					lastEnemyAction = BetAction.Call;
					OnEnemyAction?.Invoke(BetAction.Call, amountToCall, 0);
				}

				if (!enemyChips.Deduct(amountToCall))
				{
					GD.PushError("Enemy does not have enough chips for action.");
					return;
				}

				pot += amountToCall;
				enemyPut += amountToCall;

			}
			else if (choice == 1)  // fold
			{
				lastEnemyAction = BetAction.Fold;
				OnEnemyAction?.Invoke(BetAction.Fold, 0, 0);

				endedWithFold = true;
				foldedByPlayer = false;
			}
			EndPhase();
			return;
		}

		else if (!betOpen)
		{
			// TODO: Replace with AI logic and not random logic
			// No open bet -> enemy randomly Check or Raise(open)
			var choice = rng.RandiRange(0, 1); // 0 = Check, 1 = Raise
			if (choice == 0)    // check
			{
				GD.Print("Enemy Checked");
				lastEnemyAction = BetAction.Check;
				OnEnemyAction?.Invoke(BetAction.Check, 0, 0);

				// Both checked -> end
				if (lastPlayerAction == BetAction.Check)
				{
					EndPhase();
					return;
				}

				turn = Turn.Player;
				UpdateButtons();
			}
			else    // check and raise
			{
				int amount = EnemyPickRaiseAmount();
				ApplyEnemyRaise(amount);
			}
			UpdatePotLabel();
			return;
		}


		// TODO: Replace with AI logic and not random logic
		// Bet is open -> enemy randomly Call / Raise / Fold
		var r = rng.RandiRange(0, 2); // 0=Call, 1=Raise, 2=Fold
		if (r == 0)
		{
			int amountToCall = Math.Max(0, currentBet - enemyPut);

			if (amountToCall >= EnemyBalance)
			{
				// all in
				amountToCall = EnemyBalance;
				lastEnemyAction = BetAction.AllIn;
				OnEnemyAction?.Invoke(BetAction.AllIn, amountToCall, 0);

			}
			else
			{
				// regular call
				lastEnemyAction = BetAction.Call;
				OnEnemyAction?.Invoke(BetAction.Call, amountToCall, 0);
			}

			if (!enemyChips.Deduct(amountToCall))	// over here, the balance is not deducted
			{
				GD.PushError("Enemy does not have enough chips for action.");
				return;
			}
			enemyPut += amountToCall;
			pot += amountToCall;

			EndPhase(); // matched -> start battle
		}
		else if (r == 1)    // if raise
		{
			int amount = EnemyPickRaiseAmount();
			ApplyEnemyRaise(amount);
		}
		else    // if fold
		{
			lastEnemyAction = BetAction.Fold;
			OnEnemyAction?.Invoke(BetAction.Fold, 0, 0);

			endedWithFold = true;
			foldedByPlayer = false;
			EndPhase();
		}
	}

	// TODO: Replace random logic
	private int EnemyPickRaiseAmount()
	{
		int[] mults = { 1, 2, 5 };
		int m = mults[rng.RandiRange(0, mults.Length - 1)];
		int cap = Math.Min(EnemyBalance, PlayerBalance);
		return Math.Min(minimumBuyIn * m, cap);
	}

	private void ApplyEnemyRaise(int amount)
	{
		int amountToCall = Math.Max(0, currentBet - enemyPut);
		int spend = amountToCall + amount;

		if (spend >= EnemyBalance)
		{
			// all in
			spend = EnemyBalance;
			lastEnemyAction = BetAction.AllIn;
			OnEnemyAction?.Invoke(lastEnemyAction, spend, 0);
		}
		else
		{
			lastEnemyAction = amountToCall > 0 ? BetAction.CallAndRaise : BetAction.Raise;
			OnEnemyAction?.Invoke(lastEnemyAction, amountToCall, amount);
		}

		if (!enemyChips.Deduct(spend))
		{
			GD.PushError("Enemy does not have enough chips for action.");
			return;
		}

		currentBet += Math.Max(0, spend - amountToCall);
		enemyPut += spend;
		pot += spend;

		betOpen = currentBet > 0;

		// Back to player to respond
		if (lastEnemyAction == BetAction.AllIn && enemyPut == playerPut)
		{
			EndPhase();
			return;
		}

		turn = Turn.Player;
		UpdateButtons();
		UpdatePotLabel();
	}

	// ============ UI & Helpers =============
	private void EndPhase()
	{
		turn = Turn.None;
		choosingRaiseAmount = false;
		HideAll();

		if (endedWithFold)
		{
			if (!foldedByPlayer)
			{
				playerChips.AddChips(pot);
			}
			else
			{
				enemyChips.AddChips(pot);
			}
		}

		UpdatePotLabel();

		OnBetPhaseEnd?.Invoke(endedWithFold);
		GD.Print("Phase is over");
	}

	private void UpdateButtons()
	{
		HideAll();
		switch (turn)
		{
			case Turn.Player:
				if (allInButton != null)
				{
					allInButton.Text = $"All-In ($ {Math.Min(PlayerBalance, EnemyBalance)})";
					Show(allInButton);
				}
				if (choosingRaiseAmount)
				{
					if (AffordableRaise >= minimumBuyIn && EnemyBalance >= minimumBuyIn) Show(raise1xButton);
					if (AffordableRaise >= minimumBuyIn * 2 && EnemyBalance >= minimumBuyIn * 2) Show(raise2xButton);
					if (AffordableRaise >= minimumBuyIn * 5 && EnemyBalance >= minimumBuyIn * 5) Show(raise5xButton);
				}
				else if (betOpen)
				{
					if (CanCall) Show(callButton);
					if (CanRaiseOverCall) Show(raiseButton);
					Show(foldButton);
				}
				else
				{
					Show(checkButton);
					if (CanOpenRaise) Show(raiseButton);
				}
				break;

			case Turn.Enemy:

				break;
			case Turn.None:
				break;
		}
	}

	private void HideAll()
	{
		Hide(raiseButton);
		Hide(callButton);
		Hide(checkButton);
		Hide(foldButton);
		Hide(allInButton);
		Hide(raise1xButton);
		Hide(raise2xButton);
		Hide(raise5xButton);
	}

	private void Hide(Button button)
	{
		if (button == null) return;
		button.Visible = false;
		button.Disabled = true;
	}

	private void Show(Button button)
	{
		if (button == null) return;
		button.Visible = true;
		button.Disabled = false;
	}

	private void UpdatePotLabel()
	{
		OnChipsChanged?.Invoke(pot);
	}
}
