using System;
using Godot;

public partial class EffectParam : Node
{
	public BattleState State;
	public CardData Target;
	public CardData Self;
	public void Initialize(BattleState state, CardData self)
	{
		State = state;
		Self = self;
	}
}
