using Godot;
using System;

public partial class CodexEntry : TextureRect
{
    [Export] private CardBase cardBase;

    public void Initialize(CardData data)
    {
        data.Initialize();
        cardBase.Initialize(data, true);

        cardBase.EnableDefaultDrag = false;
    }
}
