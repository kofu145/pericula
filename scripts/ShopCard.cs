using Godot;
using System;
using System.Collections.Generic;

public partial class ShopCard : Control
{
    [Export] RichTextLabel labelId; // For testing purposes, to be removed
    int _cardID;
    bool _disabled = false;

    const float TWEEN_INTENSITY = 1.25f;
    const float TWEEN_DURATION = 0.25f;
    bool _isHovering = false;
    ChipManager Chips;
    DeckManager Deck;
    Shop shop;

    public override void _Ready()
    {
        Chips = GetNode<ChipManager>("/root/GlobalManager/ChipManager");
        Deck = GetNode<DeckManager>("/root/GlobalManager/DeckManager");
        base._Ready();
        PivotOffset = Size / 2;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

    public void Initialize(int id, Shop shop)
    {
        _cardID = id;
        this.shop = shop;

        // TODO: Assign icon, text, etc.
        labelId.Text = id.ToString();
    }

    void OnInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent &&
        mouseButtonEvent.IsPressed() && Input.IsActionJustPressed("click"))
        {
            OnPressed();
        }
    }

    private void OnPressed()
    {
        if (_disabled)
        {
            return;
        }
        else if (Chips.Balance >= _cardID)
        {
            Buy();
        }
    }

    private void Buy()
    {
        Chips.Deduct(_cardID);
        Deck.AddCardByID(_cardID);
        GD.Print(_cardID + " was selected!");
        GD.Print("Current Deck: " + string.Join(", ", Deck));

        shop.UpdateChipCount();
        RemoveFromShop();
    }

    private void StartTween(GodotObject @object, NodePath property, Variant finalValue, float duration, float delay = 0f)
    {
        var tween = CreateTween();
        tween.TweenProperty(
            @object,
            property,
            finalValue,
            duration).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetDelay(delay);
    }

    private void OnMouseEntered()
    {
        if (_disabled) return;
        ZIndex = 100;
        StartTween(this, "scale", Vector2.One * TWEEN_INTENSITY, TWEEN_DURATION);
    }

    public void OnMouseExited()
    {
        if (_disabled) return;
        ZIndex = 0;
        StartTween(this, "scale", Vector2.One, TWEEN_DURATION);
    }

    public void RemoveFromShop()
    {
        OnMouseExited();
        _disabled = true;
        Modulate = new Color(0.4f, 0.4f, 0.4f);
    }
}
