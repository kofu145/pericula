using Godot;
using System;

public partial class CardSlot : Control
{
    public CardLane OwnerLane { get; set; }
    public Vector2 GetCenter() => GetGlobalRect().GetCenter();

    public override void _Ready()
    {
        AddToGroup("card_slots");
    }
}
