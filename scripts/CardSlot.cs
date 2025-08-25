using Godot;
using System;

public partial class CardSlot : Control
{
	public Vector2 GetCenter() => GetGlobalRect().GetCenter();
}
