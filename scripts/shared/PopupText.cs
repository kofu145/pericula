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

    public async void ShowText(Vector2 at, string text)
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
            popup.Position.Y - 30,
            0.25f
        ).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(
            popup,
            "position:x",
            popup.Position.X + DeckManager.Instance.RndGen.Next(-20, 20),
            0.5f
        ).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(
            popup,
            "position:y",
            popup.Position.Y,
            0.5f
        ).SetEase(Tween.EaseType.Out).SetDelay(0.25f);

        tween.TweenProperty(
            popup,
            "scale",
            new Vector2(2f, 2f),
            0.25f
        ).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(
            popup,
            "scale",
            Vector2.Zero,
            0.5f
        ).SetEase(Tween.EaseType.Out).SetDelay(0.25f);

        await ToSignal(tween, "finished");
    }
}
