using Godot;

public partial class CardVisual : Control
{
    // UI refs
    [Export] private Label NameLabel;

    // runtime refs
    [Export] CardBase cardBase;
    [Export] TextureRect cardBack;
    [Export] TextureRect cardBorder;
    [Export] CardDescription cardDescription;
    [Export] Label attackLabel;
    [Export] Label healthLabel;
    public bool LerpSet => doLerp;
    public bool isFaceUp => faceUp;

    private Vector2 offsetPos;
    private float FollowSpeed = 0;
    private float ScaleSpeed = 0;
    private bool doLerp = true;
    private bool faceUp = true;
    private Color color;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        PivotOffset = Size / 2;
        cardBack.Visible = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (cardBase == null) return;

        var target = cardBase.GlobalPosition;
        float t = 1f - Mathf.Exp(-FollowSpeed * (float)delta);
        if (doLerp)
        {
            GlobalPosition = offsetPos.Lerp(target, t);

        }

        else
            GlobalPosition = cardBase.GlobalPosition;
        offsetPos = GlobalPosition;

        float ts = 1f - Mathf.Exp(-ScaleSpeed * (float)delta);
        if (doLerp)
            Scale = Scale.Lerp(cardBase.Scale, ts);

        cardBack.GlobalPosition = GlobalPosition;
        cardBack.Scale = Scale;
    }

    public void Initialize(CardData data = null, float FollowSpeed = 0, float ScaleSpeed = 0)
    {
        if (data == null) return;

        offsetPos = cardBase.Position;
        NameLabel.Text = data.DisplayName;
        cardBorder.Modulate = data.Rarity.RarityColor;

        this.FollowSpeed = FollowSpeed;
        this.ScaleSpeed = ScaleSpeed;

        if (cardBase != null)
        {
            var baseData = cardBase.GetCardData();
            healthLabel.Text = baseData.BaseHP.ToString();
            attackLabel.Text = baseData.BaseAttack.ToString();
            cardDescription.Initialize(baseData);
        }
        color = data.Rarity.RarityColor;
        SetHolo(data.Rarity.RarityType == RarityType.Mythic || data.Rarity.RarityType == RarityType.Legendary);
    }

    public void SetOffset(Vector2 pos)
    {
        offsetPos = pos;
    }

    public void RebasePos(Vector2 newPos)
    {
        cardBase.GlobalPosition = newPos;
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
        cardBack.Visible = true;
        if (faceUp)
        {
            cardBase.animation.Play("FlipCardToBack");
            //await ToSignal(Base.animation, AnimationPlayer.SignalName.AnimationFinished);
            faceUp = false;
        }
        else
        {
            cardBase.animation.Play("FlipCardToFront");
            //await ToSignal(Base.animation, AnimationPlayer.SignalName.AnimationFinished);
            faceUp = true;
            cardBack.Visible = false;
        }
    }

    public void UpdateLabels()
    {

        attackLabel.Text = cardBase.Data.Attack.ToString();
        healthLabel.Text = cardBase.Data.HP.ToString();
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
            shaderMat.SetShaderParameter("red", color.R);
            shaderMat.SetShaderParameter("blue", color.B);
            shaderMat.SetShaderParameter("green", color.G);
            Material = shaderMat;
            cardBorder.Material = shaderMat;
        }
        else
        {
            Material = null;
        }
    }
}
