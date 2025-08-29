using Godot;
using System;

public partial class CardBase : Control
{
    // visual config
    [Export] private float HoverScale = 1.2f;
    [Export] private float FollowSpeed = 12f;
    [Export] private float ScaleSpeed = 12f;

    [Export] public CardVisual Visual;
    public CardData Data;
    [Export] public CardDescription Description;
    [Export] public AnimationPlayer animation;

    public bool EnableHoverScale { get; set; } = true;
    public bool EnableDefaultDrag { get; set; } = true;

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

    public void Initialize(CardData data)
    {
        Data = data;
        if (Visual != null) Visual.Initialize(data, FollowSpeed, ScaleSpeed);
    }

    public void Reveal() => Visual?.ShowInfo();

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
            }
            else if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                OnRightClicked?.Invoke(this);
            }
            else if (_dragging)
            {
                OnEndDrag?.Invoke(this);

                _dragging = false;
                if (EnableDefaultDrag) Position = Vector2.Zero;
                AcceptEvent();
            }
        }

        else if (e is InputEventMouseMotion && _dragging)
        {
            OnDragging?.Invoke(this);

            if (EnableDefaultDrag) GlobalPosition = GetGlobalMousePosition() - _grabOffset;
            AcceptEvent();
        }
    }

    public CardData GetCardData()
    {
        return Data;
    }
}
