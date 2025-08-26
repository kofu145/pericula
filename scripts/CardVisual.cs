using Godot;

public partial class CardVisual : Control
{
	// visual config
	[Export] private float FollowSpeed = 12f;
	[Export] private float ScaleSpeed = 12f;

	// UI refs
	[Export] private Label NameLabel;

	// runtime refs
	private Control Base;

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
		GlobalPosition = GlobalPosition.Lerp(target, t);

		float ts = 1f - Mathf.Exp(-ScaleSpeed * (float)delta);
		Scale = Scale.Lerp(Base.Scale, ts);
	}

	public void Initialize(CardBase cardBase, CardData data = null)
	{
		Base = cardBase;
		if (data == null) return;

		NameLabel.Text = data.DisplayName;
	}
}
