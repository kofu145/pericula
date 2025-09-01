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
    [Export] Panel costPanel;
    [Export] Color BoughtColor;
    [Export] TextureRect image;
    [Export] Label cost;
    int _deckManipID;
    bool _disabled = false;
    bool disableDefaultHoverScale = true;

    const float TWEEN_INTENSITY = 1.25f;
    const float TWEEN_DURATION = 0.25f;
    bool _isHovering = false;

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

    public void Initialize(int id)
    {
        _deckManipID = id;

        DeckManipData data = Lookup.GetDeckManipByID(_deckManipID);
        GetNode<Label>("CardBorder/CardName").Text = data.DisplayName.ToString();
        var instance = ShopManager.Instance;
        var price = instance.GetManipPrice(id);
        var text = price.ToString();
        cost.Text = text;
        GetNode<CardDescription>("CanvasLayer/CardDescription").Initialize(data);
    }

    public void CodexInitialize(int id)
    {
        Initialize(id);
        costPanel.Visible = false;
        disableDefaultHoverScale = false;
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
                if (DeckManager.Instance.PlayerDeck.Cards.Count <= 1)
                {
                    PopupText.Instance.ShowText(GlobalPosition, "Can't remove your last card!");
                    break;
                }
                UiOverlay.Instance.Remove(RemoveCard);
                break;
            case (int)Incantation.Duplicate:
                UiOverlay.Instance.Duplicate(DuplicateCard);
                break;
            case (int)Incantation.Upgrade:
                UiOverlay.Instance.Upgrade(UpgradeCard);
                break;
            case (int)Incantation.Conjure:
                DeckManager.Instance.ConjureRandomCard();
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
        if (disableDefaultHoverScale) StartTween(this, "scale", Vector2.One * TWEEN_INTENSITY, TWEEN_DURATION);
        description.Display();
        SoundManager.PlaySE("touchcard");
    }

    public void OnMouseExited()
    {
        if (_disabled) return;
        ZIndex = 0;
        if (disableDefaultHoverScale) StartTween(this, "scale", Vector2.One, TWEEN_DURATION);
        description.Hide();
    }

    public void RemoveFromShop()
    {
        OnMouseExited();
        _disabled = true;
        image.Material = null;
        Modulate = BoughtColor;
    }
}
