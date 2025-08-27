using Godot;
using System;

public partial class CardDescription : Control
{
    [Export] RichTextLabel CardName;
    [Export] RichTextLabel Description;
    Tween tween;
    const float FINAL_SCALE = 1f;

    public override void _Ready()
    {
        
        Scale = Vector2.Zero;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        PivotOffset = Size / 2f;
    }

    public void Initialize(string name, string description)
    {
        CardName.Text = name;
        Description.Text = description;
    }

    public void Display()
    {
        Visible = true;
        if (tween != null && tween.IsRunning())
        {
            tween.Kill();
        }

        tween = CreateTween();
        tween.TweenProperty(
            this,
            "scale",
            Vector2.One * FINAL_SCALE,
            0.15f
        ).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
    }

    public void Hide()
    {
        Visible = true;
        if (tween != null && tween.IsRunning())
        {
            tween.Kill();
        }

        tween = CreateTween();
        tween.TweenProperty(
            this,
            "scale",
            Vector2.Zero,
            0.15f
        ).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
    }
}
