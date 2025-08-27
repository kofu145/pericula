using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class Deck : Resource
{
	[Export] public Godot.Collections.Array<CardData> Cards;
}
