using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class CardDescription : Control
{
    [Export] RichTextLabel CardName;
    [Export] RichTextLabel Description;
    [Export] RichTextLabel Class;
    [Export] PackedScene keywordTooltip;
    [Export] Control anchor;
    [Export] Control cardVisual;
    [Export] Vector2 tooltipOffset = new Vector2(20,20);
    
    Tween tween;
    const float FINAL_SCALE = 1f;

    Vector2 offset = new Vector2(-62f, 155f);
    private List<KeywordTooltip> keywords = new();

    public override void _Ready()
    {
        Scale = Vector2.Zero;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        PivotOffset = Size / 2f;
        GlobalPosition = GetGlobalMousePosition() + tooltipOffset;
    }

    public void Initialize(CardData data)
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
        
        CardName.Text = data.DisplayName;
        Description.Text = data.Description;
        Class.Text = data.Trait;

        foreach (var keyword in data.keywords)
        {
            var tooltip = keywordTooltip.Instantiate<KeywordTooltip>();
            tooltip.Initialize(keyword);
            anchor.AddChild(tooltip);
            keywords.Add(tooltip);
        }
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
