using Godot;

public partial class CardVisual : Control
{
	// UI refs
	[Export] private Label NameLabel;

	// runtime refs
	[Export] Control Base;

	private Vector2 offsetPos;
	private float FollowSpeed = 0;
	private float ScaleSpeed = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
		PivotOffset = Size / 2;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Base == null) return;

		var target = Base.GlobalPosition;
		float t = 1f - Mathf.Exp(-FollowSpeed * (float)delta);
		GlobalPosition = offsetPos.Lerp(target, t);
		offsetPos = GlobalPosition;

		float ts = 1f - Mathf.Exp(-ScaleSpeed * (float)delta);
		Scale = Scale.Lerp(Base.Scale, ts);
	}

	public void Initialize(CardData data = null, float FollowSpeed = 0, float ScaleSpeed = 0)
	{
		if (data == null) return;

		offsetPos = Base.Position;
		NameLabel.Text = data.DisplayName;

		this.FollowSpeed = FollowSpeed;
		this.ScaleSpeed = ScaleSpeed;
	}

	public void RebasePos(Vector2 newPos)
	{
		Base.GlobalPosition = newPos;
	}

	public void HideInfo()
	{

	}

	/// <summary>
	/// Reveal the card's info (flip card face up)
	/// </summary>
	public void ShowInfo()
	{

	}
}
