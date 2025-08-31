using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

public partial class CardDescription : Control
{
    [Export] RichTextLabel CardName;
    [Export] RichTextLabel Description;
    [Export] RichTextLabel Class;
    [Export] PackedScene keywordTooltip;
    [Export] Control anchor;
    [Export] Vector2 tooltipOffset = new Vector2(20, 20);
    [Export] public Color keywordColor = new Color(0, 0, 0);


    [Export] public Color KnightColor = new Color(1, 1, 1);
    [Export] public Color ArcaneColor = new Color(1, 1, 1);
    [Export] public Color CitizenColor = new Color(1, 1, 1);
    [Export] public Color RoyaltyColor = new Color(1, 1, 1);
    [Export] public Color MechanicalColor = new Color(1, 1, 1);
    [Export] public Color WildCardColor = new Color(1, 1, 1);
    [Export] public Color PawnColor = new Color(1, 1, 1);
    [Export] public Color UndeadColor = new Color(1, 1, 1);

    private Dictionary<Trait, Color> traitColors = new();


    Tween tween;
    const float FINAL_SCALE = 1f;
    private List<KeywordTooltip> keywords = new();

    public override void _Ready()
    {
        BuildTraitToColorDictionary();
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
        Description.BbcodeEnabled = true;
        Class.BbcodeEnabled = true;

        CardName.Text = data.DisplayName;
        string desc = data.Description;

        // Keyword coloring
        string keywordHex = keywordColor.ToHtml(true);
        foreach (var keyword in data.Keywords)
            desc = desc.Replace(keyword.DisplayName, $"[color={keywordHex}]{keyword.DisplayName}[/color]");

        // Ensure trait color map is ready 
        if (traitColors == null || traitColors.Count == 0)
            BuildTraitToColorDictionary();

        // Self trait color (use hex)
        if (traitColors.TryGetValue(data.Trait, out var selfColor))
        {
            string selfHex = selfColor.ToHtml(true);
            Class.Text = $"[color={selfHex}]{data.Trait}[/color]";
        }
        else
        {
            Class.Text = data.Trait.ToString(); // fallback
        }

        // replace trait to trait color in description
        var traitNames = Enum.GetNames(typeof(Trait));
        var escaped = Array.ConvertAll(traitNames, Regex.Escape);
        string pattern = $@"\b({string.Join("|", escaped)})(es|s)?\b";


        desc = Regex.Replace(
                   desc,
                   pattern,
                   m =>
                   {
                       string baseWord = m.Groups[1].Value;
                       string suffix = m.Groups[2].Success ? m.Groups[2].Value : "";

                       if (Enum.TryParse<Trait>(baseWord, out var trait)
                           && traitColors.TryGetValue(trait, out var color))
                       {
                           string hex = color.ToHtml(true);
                           return $"[color={hex}]{baseWord + suffix}[/color]";
                       }

                       return m.Value; // fallback
                   }
               );


        Description.Text = desc;

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

    private void BuildTraitToColorDictionary()
    {
        traitColors = new()
        {
            { Trait.Knight, KnightColor },
            { Trait.Arcane, ArcaneColor },
            { Trait.Citizen, CitizenColor },
            { Trait.Royalty, RoyaltyColor },
            { Trait.Mechanical, MechanicalColor },
            { Trait.WildCard, WildCardColor },
            { Trait.Pawn, PawnColor },
            { Trait.Undead, UndeadColor },
        };
    }
}
