using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

public partial class CardDescription : Control
{
    // UI refs
    [Export] RichTextLabel CardName;
    [Export] RichTextLabel Description;
    [Export] RichTextLabel Class;
    [Export] RichTextLabel Rarity;

    [Export] PackedScene keywordTooltip;

    // optional parameter to offset tooltip from the mouse position
    [Export] Control anchor;
    [Export] Vector2 tooltipOffset = new Vector2(20, 20);

    // lookup for all rarities and keywords
    [Export] private Godot.Collections.Array<Rarity> rarities;
    [Export] private Godot.Collections.Array<Keyword> keywords;

    // color settings for the word 'Rarity'
    [Export] public Color rarityWordColor = new Color(0, 0, 0);
    // color settings for keywords
    [Export] public Color keywordColor = new Color(0, 0, 0);

    // color settings for each traits
    [Export] public Color KnightColor = new Color(1, 1, 1);
    [Export] public Color ArcaneColor = new Color(1, 1, 1);
    [Export] public Color CitizenColor = new Color(1, 1, 1);
    [Export] public Color RoyaltyColor = new Color(1, 1, 1);
    [Export] public Color MechanicalColor = new Color(1, 1, 1);
    [Export] public Color WildCardColor = new Color(1, 1, 1);
    [Export] public Color PawnColor = new Color(1, 1, 1);
    [Export] public Color UndeadColor = new Color(1, 1, 1);

    // mapping of trait to color
    private Dictionary<Trait, Color> traitColors = new();

    Tween tween;
    const float FINAL_SCALE = 1f;

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

        // ------------------ Update description -----------

        // replace the word 'Rarity'
        desc = Regex.Replace(
            desc,
            @"\brarity\b",
            m => $"[color={rarityWordColor.ToHtml(true)}]{m.Value}[/color]",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );

        // Ensure trait color map is ready 
        if (traitColors == null || traitColors.Count == 0)
            BuildTraitToColorDictionary();

        // Self trait color (use hex)
        if (traitColors.TryGetValue(data.Trait, out var selfColor))
        {
            string selfHex = selfColor.ToHtml(true);
            string spaced = Regex.Replace(data.Trait.ToString(), "(\\B[A-Z])", " $1");

            if (spaced.ToLower() == "wild card") spaced = $"[wave][rainbow freq=0.2 sat=10 val=20]{spaced}[/rainbow][/wave]";
            Class.Text = $"[color={selfHex}]{spaced}[/color]";
        }
        else
        {
            Class.Text = data.Trait.ToString(); // fallback
        }

        // replace trait to trait color in description
        var traitNames = Enum.GetNames(typeof(Trait));
        var escaped = Array.ConvertAll(traitNames, AddSpacePattern);

        string pattern = $@"\b({string.Join("|", escaped)})(es|s)?\b";

        desc = Regex.Replace(
                   desc,
                   pattern,
                   m =>
                   {
                       string baseWord = m.Groups[1].Value;
                       string suffix = m.Groups[2].Success ? m.Groups[2].Value : "";

                       string traitName = Regex.Replace(baseWord, @"[\s_-]+", "");
                       if (Enum.TryParse<Trait>(traitName, out var trait)
                           && traitColors.TryGetValue(trait, out var color))
                       {
                           string spaced = Regex.Replace(baseWord + suffix, "(\\B[A-Z])", " $1");   // adds a space between wild and card
                           string hex = color.ToHtml(true);
                           if (trait.ToString().ToLower() == "wildcard") spaced = $"[rainbow freq=0.2 sat=10 val=20]{spaced}[/rainbow]";
                           return $"[color={hex}]{spaced}[/color]";
                       }

                       return m.Value; // fallback
                   }
               );


        // replace rarity to rarity color in description
        if (rarities != null && rarities.Count > 0)
        {
            foreach (var r in rarities)
            {
                if (r == null || string.IsNullOrWhiteSpace(r.DisplayName)) continue;

                string name = r.DisplayName;
                string hex = r.RarityColor.ToHtml(true);

                string pat;
                if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase))
                {
                    // e.g., legendary / legendaries
                    var stem = Regex.Escape(name.Substring(0, name.Length - 1));
                    pat = $@"\b{stem}(?:y|ies)\b";
                }
                else
                {
                    // e.g., common(s), rare(s), mythic(s)
                    var escapedName = Regex.Escape(name);
                    pat = $@"\b{escapedName}s?\b";
                }

                desc = Regex.Replace(
                    desc,
                    pat,
                    m => $"[color={hex}]{m.Value}[/color]",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
                );
            }
        }

        Description.Text = desc;
        // -------------------------------------

        var rarityColor = data.Rarity.RarityColor.ToHtml(true);
        Rarity.Text = $"[color={rarityColor}]{data.Rarity.DisplayName}[/color]";

        foreach (var keyword in data.Keywords)
        {
            var tooltip = keywordTooltip.Instantiate<KeywordTooltip>();
            tooltip.Initialize(keyword);
            anchor.AddChild(tooltip);
        }
    }

    public void Initialize(DeckManipData data)
    {
        Rarity.Visible = false;
        CardName.Text = data.DisplayName;
        Class.Text = "Incantation";
        var desc = data.Description;

        // replace the word 'Rarity'
        desc = Regex.Replace(
            desc,
            @"\brarity\b",
            m => $"[color={rarityWordColor.ToHtml(true)}]{m.Value}[/color]",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );

        if (rarities != null && rarities.Count > 0)
        {
            foreach (var r in rarities)
            {
                if (r == null || string.IsNullOrWhiteSpace(r.DisplayName)) continue;

                string name = r.DisplayName;
                string hex = r.RarityColor.ToHtml(true);

                string pat;
                if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase))
                {
                    // e.g., legendary / legendaries
                    var stem = Regex.Escape(name.Substring(0, name.Length - 1));
                    pat = $@"\b{stem}(?:y|ies)\b";
                }
                else
                {
                    // e.g., common(s), rare(s), mythic(s)
                    var escapedName = Regex.Escape(name);
                    pat = $@"\b{escapedName}s?\b";
                }

                desc = Regex.Replace(
                    desc,
                    pat,
                    m => $"[color={hex}]{m.Value}[/color]",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
                );
            }
        }
        Description.Text = desc;
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

    private string AddSpacePattern(string name)
    {
        // Escape enum name and allow optional spaces before uppercase letters
        return Regex.Replace(Regex.Escape(name), "(\\B[A-Z])", @"\s*$1");
    }
}
