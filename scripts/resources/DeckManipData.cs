using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DeckManipData : Resource
{
    [Export] public int id;
    [Export] public string DisplayName;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public int Cost;
    [Export] public Texture2D Texture;
}
