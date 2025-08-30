using Godot;
using System;
using System.Collections.Generic;

public partial class ShopCard : Control
{
    [Export] CardDescription description;
    [Export] private TextureRect border;
    [Export] private TextureRect baseCard;
    [Export] private TextureRect spriteImage;
    [Export] private Label nameLabel;
    [Export] private Label healthLabel;
    [Export] private Label attackLabel;
    [Export] private Label costLabel;
    int _cardID;
    bool _disabled = false;

    const float TWEEN_INTENSITY = 1.25f;
    const float TWEEN_DURATION = 0.25f;
    bool _isHovering = false;
    CardData data;
    Shop shop;
    private Color color;

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

        data = Lookup.GetCardByID(_cardID);
        nameLabel.Text = data.DisplayName;
        healthLabel.Text = data.BaseHP.ToString();
        attackLabel.Text = data.BaseAttack.ToString();
        border.Modulate = data.Rarity.RarityColor;
        spriteImage.Texture = data.Texture;
        color = data.Rarity.RarityColor;

        costLabel.Text = ShopManager.Instance.GetCardPrice(data).ToString();
        var rarity = data.Rarity.RarityType;
        SetHolo(rarity == RarityType.Mythic || rarity == RarityType.Legendary);

        description.Initialize(data);
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
        else if (ChipManager.Instance.Deduct(ShopManager.Instance.GetCardPrice(data)))
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

    public void SetHolo(bool isHolo)
    {
        if (isHolo)
        {
            Shader shader = GD.Load<Shader>("res://scripts/shaders/movingrainbow.gdshader");
            ShaderMaterial shaderMat = new();
            shaderMat.Shader = shader;
            shaderMat.SetShaderParameter("strength", 0.12);
            shaderMat.SetShaderParameter("speed", 0.3);
            shaderMat.SetShaderParameter("angle", 45);
            shaderMat.SetShaderParameter("red", color.R);
            shaderMat.SetShaderParameter("blue", color.B);
            shaderMat.SetShaderParameter("green", color.G);
            baseCard.Material = shaderMat;
            border.Material = shaderMat;
        }
        else
        {
            baseCard.Material = null;
        }
    }
}
