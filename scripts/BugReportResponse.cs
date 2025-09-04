using Godot;
using System;

public partial class BugReportResponse : Control
{
    [Export] private Label responseLabel;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionJustPressed("escape"))
        {
            Close();
        }
    }
    public void Initialize(bool isSuccess)
    {
        if (responseLabel != null) responseLabel.Text = isSuccess ? "Reported Submitted!" : "Report failed to send.";
    }
    public void Close() => QueueFree();
}
