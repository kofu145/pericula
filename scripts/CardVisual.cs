using Godot;

public partial class CardVisual : Control
{
	private Control Base;
	[Export] private float FollowSpeed = 12f;
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
	}

	public void Initialize(CardBase cardBase)
	{
		Base = cardBase;
	}
}
