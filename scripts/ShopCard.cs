using Godot;
using System;
using System.Collections.Generic;

public partial class ShopCard : Control
{
    [Export] CardDescription description;
    int _cardID;
    bool _disabled = false;

    const float TWEEN_INTENSITY = 1.25f;
    const float TWEEN_DURATION = 0.25f;
    bool _isHovering = false;
    Shop shop;

    public override void _Ready()
    {
        base._Ready();
        PivotOffset = Size / 2;

        StartTween(this, "scale", Vector2.One * TWEEN_INTENSITY, TWEEN_DURATION / 2f);
        StartTween(this, "scale", Vector2.One, TWEEN_DURATION, TWEEN_DURATION / 2f);
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
        CardData data = Lookup.GetCardByID(_cardID);
        GetNode<Label>("CardBorder/Health").Text = data.BaseHP.ToString();
        GetNode<Label>("CardBorder/Attack").Text = data.BaseAttack.ToString();

        double _currentCost = 100;

        // TODO: Update this later
        switch (data.Rarity.RarityType)
        {
            case RarityType.Rare:
                _currentCost *= 1.5;
                break;
            case RarityType.Mythic:
                _currentCost *= 2;
                break;
            case RarityType.Legendary:
                _currentCost *= 2.5;
                break;
            default:
                break;
        }

        GetNode<Label>("CardBorder/Cost").Text = Math.Round(_currentCost).ToString();
        GetNode<CardDescription>("CanvasLayer/CardDescription").Initialize(data);
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
        else if (ChipManager.Instance.Balance >= _cardID)
        {
            Buy();
        }
        else
        {
            PopupText.Instance.ShowText(GlobalPosition, "Too expensive!");
        }
    }

    private void Buy()
    {
        ChipManager.Instance.Deduct(_cardID);
        DeckManager.Instance.AddCardByID(_cardID);
        GD.Print(_cardID + " was selected!");
        GD.Print("Current Deck: " + string.Join(", ", DeckManager.Instance.PlayerDeck));
        PopupText.Instance.ShowText(GlobalPosition, "Purchased!");

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
        description.Display();
        SoundManager.PlaySE("touchcard");
    }

    public void OnMouseExited()
    {
        if (_disabled) return;
        ZIndex = 0;
        StartTween(this, "scale", Vector2.One, TWEEN_DURATION);
        description.Hide();
    }

    public void RemoveFromShop()
    {
        OnMouseExited();
        _disabled = true;
        Modulate = new Color(0.4f, 0.4f, 0.4f);
    }
}
