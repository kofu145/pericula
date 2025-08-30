using Godot;
using System;

public partial class CardBase : Control
{
    // visual config
    [Export] private float HoverScale = 1.2f;
    [Export] private float FollowSpeed = 12f;
    [Export] private float ScaleSpeed = 12f;

    [Export] public CardVisual Visual;
    [Export] public TextureRect CardImage;
    public CardData Data;
    [Export] public CardDescription Description;
    [Export] public AnimationPlayer animation;

    public bool EnableHoverScale { get; set; } = true;
    public bool EnableDefaultDrag { get; set; } = true;
    public bool IsPlayer = true;

    // public events
    public event Action<CardBase> OnLeftClicked;
    public event Action<CardBase> OnRightClicked;
    public event Action<CardBase> OnHoverEntered;
    public event Action<CardBase> OnHoverExited;
    public event Action<CardBase> OnStartDrag;
    public event Action<CardBase> OnDragging;
    public event Action<CardBase> OnEndDrag;

    // runtime references
    private bool _dragging;
    private Vector2 _grabOffset;
    private int originIndex;
    private CardLane originLane;

    public void Initialize(CardData data)
    {
        Data = data;
        if (Visual != null) Visual.Initialize(data, FollowSpeed, ScaleSpeed);
        var cardTex = new Godot.Sprite2D();
        //cardTex.Texture = Data.Texture;
        //cardTex.Position = new Vector2(60, 82);
        //cardTex.Scale = new Vector2(3.125f, 3.125f);
        CardImage.Texture = Data.Texture;
        CardImage.Position = new Vector2(10, 32.5f);
        SetHolo(true);

    }

    public void FlipCard() => Visual?.FlipCard();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        PivotOffset = Size / 2;
        MouseFilter = MouseFilterEnum.Stop;

        // Invoke events
        MouseEntered += () =>
        {
            OnHoverEntered?.Invoke(this);
            if (EnableHoverScale) Scale = new Vector2(HoverScale, HoverScale);

            // Show card description
            if (Visual.isFaceUp || IsPlayer)
                Description.Display();
            ZIndex = 10;
        };
        MouseExited += () =>
        {
            OnHoverExited?.Invoke(this);
            if (EnableHoverScale) Scale = new Vector2(1, 1);

            Description.Hide();
            ZIndex = 0;
        };
    }

    public override void _GuiInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                OnLeftClicked?.Invoke(this);
                OnStartDrag?.Invoke(this);

                _dragging = true;
                _grabOffset = GetGlobalMousePosition() - GlobalPosition;

                var parentSlot = GetParent() as CardSlot;
                originLane = parentSlot?.OwnerLane;
                originIndex = originLane != null ? originLane.IndexOf(this) : -1;
            }
            else if (mb.ButtonIndex == MouseButton.Right && mb.Pressed)
            {
                OnRightClicked?.Invoke(this);
            }
            else if (mb.ButtonIndex == MouseButton.Left && !mb.IsPressed())
            {
                if (_dragging)
                {
                    // dropped
                    OnEndDrag?.Invoke(this);
                    _dragging = false;

                    // Try to place/swap into hovered slot
                    // var targetSlot = GetHoveredSlot();

                    if (EnableDefaultDrag) Position = Vector2.Zero;
                    AcceptEvent();
                }
            }
        }

        else if (e is InputEventMouseMotion && _dragging)
        {
            OnDragging?.Invoke(this);

            if (EnableDefaultDrag) GlobalPosition = GetGlobalMousePosition() - _grabOffset;
            AcceptEvent();
        }
    }

    public void SetHolo(bool isHolo)
    {
        if (isHolo)
        {
            Shader shader = GD.Load<Shader>("res://scripts/shaders/movingrainbow.gdshader");
            ShaderMaterial shaderMat = new();
            shaderMat.Shader = shader;
            shaderMat.SetShaderParameter("strength", 0.12);
            shaderMat.SetShaderParameter("speed", 0.3);
            shaderMat.SetShaderParameter("angle", 45);
            Visual.Material = shaderMat;
        }
        else
        {
            Visual.Material = null;
        }
    }

    public CardData GetCardData()
    {
        return Data;
    }

    private CardSlot GetHoveredSlot()
    {
        Vector2 mouse = GetGlobalMousePosition();
        foreach (var n in GetTree().GetNodesInGroup("card_slots"))
        {
            if (n is CardSlot s && s.GetGlobalRect().HasPoint(mouse))
                return s;
        }
        return null;
    }
}
