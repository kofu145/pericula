using Godot;

public abstract partial class CodexItemData : Resource
{
    [Export] public int id;
    [Export] public string DisplayName;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public Texture2D Texture;

    /// Sort key hook. Override in derived types.
    public virtual (int a, int b, int c) GetSortKey() => (0, 0, id);
}
