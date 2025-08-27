using Godot;
using System;

public partial class PopupText : Node
{
	[Export] private PackedScene PopupTextScene;

	public static PopupText Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
	}

	public void ShowNumber(Vector2 at, int number)
	{
		ShowText(at, number.ToString());
	}

	public void ShowText(Vector2 at, string text)
	{
		var popup = PopupTextScene.Instantiate<Label>();
		popup.GlobalPosition = at;
		popup.ZIndex = 1000;
		popup.Text = text;
		popup.PivotOffset = popup.Size / 2;
		GetTree().Root.AddChild(popup);

		var tween = GetTree().CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(
			popup,
			"position:y",
			popup.Position.Y - 40,
			.25).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(
			popup,
			"position:y",
			popup.Position.Y,
			.5).SetEase(Tween.EaseType.In).SetDelay(.25);
		tween.TweenProperty(
			popup,
			"position:x",
			popup.Position.X + DeckManager.Instance.RndGen.Next(-30, 30),
			.25).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(
			popup,
			"scale",
			Vector2.One * 0.25f,
			.25).SetEase(Tween.EaseType.In).SetDelay(.5);
		tween.TweenCallback(Callable.From(popup.QueueFree)).SetDelay(.7);
	}
}
