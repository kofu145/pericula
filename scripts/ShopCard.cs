using Godot;
using System;
using System.Collections.Generic;

public partial class ShopCard : Control
{
	[Export] RichTextLabel labelId; // For testing purposes, to be removed
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
		CardData data = CardLookup.GetCardByID(_cardID);
		GetNode<Label>("Health").Text = data.BaseHP.ToString();
		GetNode<Label>("Attack").Text = data.BaseAttack.ToString();

		double _currentCost = 100;

		// TODO: Update this later
		switch (data.Rarity)
		{
			case "Rare":
				_currentCost *= 1.5;
				break;
			case "Mythic":
				_currentCost *= 2;
				break;
			case "Legendary":
				_currentCost *= 2.5;
				break;
			default:
				break;
		}

		GetNode<Label>("Cost").Text = Math.Round(_currentCost).ToString();
		GetNode<CardDescription>("CanvasLayer/CardDescription").Initialize(data.DisplayName, data.Description, data.Rarity, data.Trait);
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
	}

	private void Buy()
	{
		ChipManager.Instance.Deduct(_cardID);
		DeckManager.Instance.AddCardByID(_cardID);
		GD.Print(_cardID + " was selected!");
		GD.Print("Current Deck: " + string.Join(", ", DeckManager.Instance.PlayerDeck));
		PopupText.Instance.ShowText(GlobalPosition, "Purchased!");

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
		description.Display();
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
