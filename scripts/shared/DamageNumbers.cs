using Godot;
using System;

public partial class DamageNumbers : Node
{
	[Export] private PackedScene DamageNumberScene;

	public static DamageNumbers Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
	}

	public void CreateNumber(Vector2 at, int number)
	{
		var num = DamageNumberScene.Instantiate<Label>();
		num.GlobalPosition = at;
		num.ZIndex = 1000;
		num.Text = number.ToString();
		GetTree().Root.FindChild("Canvas", true, false).AddChild(num);

		var tween = GetTree().CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(num, "position:y", num.Position.Y - 10, .25).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(num, "position:y", num.Position.Y, .5).SetEase(Tween.EaseType.In).SetDelay(.25);
		tween.TweenProperty(num, "position:x", num.Position.X + DeckManager.Instance.RndGen.Next(-10, 10), .25).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(num, "scale", new Vector2(.5f, .5f), .25).SetEase(Tween.EaseType.In).SetDelay(.5);


		tween.TweenCallback(Callable.From(() => num.QueueFree())).SetDelay(.7);
		//await ToSignal(tween, Tween.SignalName.Finished);
		//num.QueueFree();
	}
}
