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
    [Export] Vector2 tooltipOffset = new Vector2(20, 20);
    
    Tween tween;
    const float FINAL_SCALE = 1f;
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
        CardName.Text = data.DisplayName;
        Description.Text = data.Description;
        Class.Text = data.Trait;

        foreach (var keyword in data.Keywords)
        {
            var tooltip = keywordTooltip.Instantiate<KeywordTooltip>();
            tooltip.Initialize(keyword);
            anchor.AddChild(tooltip);
            keywords.Add(tooltip);
        }
    }

    public void Initialize(DeckManipData data)
    {
        CardName.Text = data.DisplayName;
        Description.Text = data.Description;
        Class.Text = "Incantation";
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
