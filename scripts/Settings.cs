using Godot;
using System;

public partial class Settings : Panel
{

    public override void _Ready()
    {
        base._Ready();
        Visible = false;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionJustPressed("escape"))
        {
            ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        Visible = !Visible;
    }
}
