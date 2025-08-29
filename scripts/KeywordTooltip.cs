using Godot;
using System;

public partial class KeywordTooltip : Control
{
    [Export] RichTextLabel KeywordName;
    [Export] RichTextLabel Description;

    public void Initialize(Keyword data)
    {
        KeywordName.Text = data.DisplayName;
        Description.Text = data.Description;
    }
}
