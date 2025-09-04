using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DeckManipData : CodexItemData
{
    [Export] public int Cost;
    [Export] public Color color;
    
    public int TimesUsed { get; private set; }
    public void Use() => TimesUsed++;
    public void Reset() => TimesUsed = 0;

    public override (int a, int b, int c) GetSortKey()
    {
        return (Cost, 0, id);
    }
}
