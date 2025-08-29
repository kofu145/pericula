using Godot;
using System;

public partial class ViewDeckButtonBinder : Control
{
    [Export] private Button drawPileButton;
    [Export] private Button discardPileButton;
    public override void _Ready()
    {
        UiOverlay.Instance.BindDraw(drawPileButton);
        UiOverlay.Instance.BindDiscard(discardPileButton);
    }
}
