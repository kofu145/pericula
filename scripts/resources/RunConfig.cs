using Godot;
using System;

[GlobalClass]
public partial class RunConfig : Resource
{
    [Export] public int playerStartingChips = 100;
    [Export] public Deck playerStartingDeck;
}
