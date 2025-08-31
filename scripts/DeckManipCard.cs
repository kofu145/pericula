using Godot;
using System;
using System.Collections.Generic;

public partial class DeckManipCard : Control
{
    enum Incantation : int
    {
        Remove = 1,
        Duplicate = 2,
        Upgrade = 3,
        Conjure = 4,

    }
    [Export] CardDescription description;
    int _deckManipID;
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
        _deckManipID = id;
        this.shop = shop;

        DeckManipData data = Lookup.GetDeckManipByID(_deckManipID);
        GetNode<Label>("CardBorder/CardName").Text = data.DisplayName.ToString();
        GetNode<Label>("CardBorder/Cost").Text = ShopManager.Instance.GetManipPrice(id).ToString();
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
        else if (ChipManager.Instance.Deduct(ShopManager.Instance.GetManipPrice(_deckManipID)))
        {
            Buy();
        }
        else
        {
            PopupText.Instance.ShowText(GlobalPosition, "Too expensive!");
            SoundManager.PlaySE("fail");
        }
    }

    private void Buy()
    {
        switch (_deckManipID)
        {
            case (int)Incantation.Remove:
                UiOverlay.Instance.Remove(RemoveCard);
                break;
            case (int)Incantation.Duplicate:
                UiOverlay.Instance.Duplicate(DuplicateCard);
                break;
            case (int)Incantation.Upgrade:
                UiOverlay.Instance.Upgrade(UpgradeCard);
                break;
            case (int)Incantation.Conjure:
                DeckManager.Instance.AddRandomCard();
                break;
        }

        PopupText.Instance.ShowText(GlobalPosition, "Purchased!");
        SoundManager.PlaySE("bought_item");
        RemoveFromShop();
    }

    private void RemoveCard(CardBase cardBase)
    {
        DeckManager.Instance.Remove(cardBase.Data);
        GD.Print($"Remove card: {cardBase.Data.DisplayName}");
    }

    private void DuplicateCard(CardBase cardBase)
    {
        DeckManager.Instance.DuplicateCard(cardBase.Data);
        GD.Print($"duplicate card: {cardBase.Data.DisplayName}");
    }

    private void UpgradeCard(CardBase cardBase)
    {
        DeckManager.Instance.Upgrade(cardBase.Data);
        GD.Print($"Upgrade card: {cardBase.Data.DisplayName}");
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
