using Godot;
using System;

public partial class EventBus : Node
{
	public static EventBus Instance { get; private set; }
	public override void _Ready()
	{
		Instance = this;
	}

	//[Signal]
	public delegate void EventHandler();

	//public event EventHandler OnDamage;

	public event Action BasicAttackAnimEvent;

	public void EnqueueAttackAnimation()
	{
		BasicAttackAnimEvent?.Invoke();
	}

}
