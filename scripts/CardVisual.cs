using Godot;

public partial class CardVisual : Control
{
	// visual config
	[Export] private float FollowSpeed = 12f;
	[Export] private float ScaleSpeed = 12f;

	// UI refs
	[Export] private Label NameLabel;

	// runtime refs
	[Export] Control Base;

	private Vector2 offsetPos;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
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

	public void Initialize(CardBase cardBase, CardData data = null)
	{
		Base = cardBase;
		if (data == null) return;
		offsetPos = cardBase.Position;
		NameLabel.Text = data.DisplayName;
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
