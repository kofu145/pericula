using Godot;
using System;
using System.Collections;
using System.Runtime.CompilerServices;

public partial class CardDescription : Control
{
    [Export] RichTextLabel CardName;
    [Export] RichTextLabel Description;
    [Export] RichTextLabel Class;
    [Export] Control cardVisual;
    Tween tween;
    const float FINAL_SCALE = 1f;

    Vector2 offset = new Vector2(-62f, 155f);

    public override void _Ready()
    {
        Scale = Vector2.Zero;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        PivotOffset = Size / 2f;
        GlobalPosition = GetGlobalMousePosition();
    }

    public void Initialize(string name, string description, string rarity, string className)
    {
        // switch (rarity)
        // {
        //     case "common":
        //         CardName.Theme.SetColor("RichTextLabel", "default_color", Colors.DarkGreen);
        //         break;
        //     case "rare":
        //         CardName.Theme.SetColor("RichTextLabel", "default_color", Colors.Blue);
        //         break;
        //     case "mythic":
        //         CardName.Theme.SetColor("RichTextLabel", "default_color", Colors.Purple);
        //         break;
        //     case "legendary":
        //         CardName.Theme.SetColor("RichTextLabel", "default_color", Colors.Gold);
        //         break;
        //     case "starter":
        //         CardName.Theme.SetColor("RichTextLabel", "default_color", Colors.Gray);
        //         break;
        //     default:
        //         CardName.Theme.SetColor("RichTextLabel", "default_color", Colors.Gray);
        //         break;
        // }

        CardName.Text = name;
        Description.Text = description;
        Class.Text = className;
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
