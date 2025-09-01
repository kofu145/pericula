using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DeckManipData : CodexItemData
{
    [Export] public int Cost;
    [Export] public Color color;

    public override (int a, int b, int c) GetSortKey()
    {
        return (Cost, 0, id);
    }
}
