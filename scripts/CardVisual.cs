using Godot;

public partial class CardVisual : Control
{
    // UI refs
    [Export] private Label NameLabel;

    // runtime refs
    [Export] CardBase Base;
    [Export] TextureRect CardBack;

    private Vector2 offsetPos;
    private float FollowSpeed = 0;
    private float ScaleSpeed = 0;
    private bool doLerp = true;
    private bool faceUp = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        PivotOffset = Size / 2;
        CardBack.Visible = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Base == null) return;

        var target = Base.GlobalPosition;
        float t = 1f - Mathf.Exp(-FollowSpeed * (float)delta);
        if (doLerp)
        {
            GlobalPosition = offsetPos.Lerp(target, t);

        }

        else
            GlobalPosition = Base.GlobalPosition;
        offsetPos = GlobalPosition;

        float ts = 1f - Mathf.Exp(-ScaleSpeed * (float)delta);
        if (doLerp)
            Scale = Scale.Lerp(Base.Scale, ts);

        CardBack.GlobalPosition = GlobalPosition;
        CardBack.Scale = Scale;
    }

    public void Initialize(CardData data = null, float FollowSpeed = 0, float ScaleSpeed = 0)
    {
        if (data == null) return;

        offsetPos = Base.Position;
        NameLabel.Text = data.DisplayName;

        this.FollowSpeed = FollowSpeed;
        this.ScaleSpeed = ScaleSpeed;

        if (Base != null)
        {
            var baseData = Base.GetCardData();
            GetNode<Label>("Health").Text = baseData.BaseHP.ToString();
            GetNode<Label>("Attack").Text = baseData.BaseAttack.ToString();
            GetNode<CardDescription>("CanvasLayer/CardDescription").Initialize(baseData);
        }
    }

    public void RebasePos(Vector2 newPos)
    {
        Base.GlobalPosition = newPos;
    }

    public void ToggleLerp(bool value)
    {
        doLerp = value;
    }

    /// <summary>
    /// Toggles flipping the card
    /// </summary>
    public async void FlipCard()
    {
        CardBack.Visible = true;
        if (faceUp)
        {
            Base.animation.Play("FlipCardToBack");
            await ToSignal(Base.animation, AnimationPlayer.SignalName.AnimationFinished);
            faceUp = false;
        }
        else
        {
            Base.animation.Play("FlipCardToFront");
            await ToSignal(Base.animation, AnimationPlayer.SignalName.AnimationFinished);
            faceUp = true;
            CardBack.Visible = false;
        }
    }
}
