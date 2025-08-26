using Godot;
using System;

public partial class DeckToggle : TextureButton
{
	// UI refs
	[Export] private TextureButton ViewDeckButton;
	[Export] private DeckListView DeckPanel;
	[Export] private Control DeckOverlay;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ViewDeckButton.Pressed += ToggleDeckView;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _UnhandledInput(InputEvent e)
	{
		if (DeckPanel.Visible && Input.IsActionJustPressed("ui_cancel")) CloseDeckView();
	}

	private void ToggleDeckView()
	{
		if (DeckPanel.Visible) CloseDeckView();
		else OpenDeckView();
	}

	private void OpenDeckView()
	{
		if (DeckOverlay != null) DeckOverlay.Visible = true;
		DeckPanel.Visible = true;
		DeckPanel.GrabFocus();
	}

	private void CloseDeckView()
	{
		DeckPanel.Visible = false;
	}
}
