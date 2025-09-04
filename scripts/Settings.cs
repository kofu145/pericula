using Godot;

public partial class Settings : Panel
{
    // UI refs
    [Export] private Button titleScreenButton;
    [Export] private Button restartRunButton;
    [Export] private Godot.Collections.Array<string> hideTitleScreenButtonOn;
    [Export] private Godot.Collections.Array<string> hiderestartRunButtonOn;
    [Export] private BugReport bugReport;


    public override void _Ready()
    {
        base._Ready();
        Visible = false;
        var currentScene = GetTree().CurrentScene.Name;
        GD.Print(currentScene);
        if (titleScreenButton != null) titleScreenButton.Visible = !hideTitleScreenButtonOn.Contains(currentScene);
        if (restartRunButton != null) restartRunButton.Visible = !hiderestartRunButtonOn.Contains(currentScene);
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
        bugReport.Visible = false;
    }

    public void SwitchToBugReport()
    {
        bugReport.Visible = true;
        Visible = false;
    }

    public void ReturnToTitleScreen()
    {
        SceneManager.ChangeSceneToFile("TitleScreen");
        Visible = false;
    }

    public void RestartRun()
    {
        RunManager.Instance.StartRun();
        Visible = false;
    }

    public void Refresh(string sceneName)
    {
        var scene = sceneName.Split("/")[1];
        scene = scene.Split('.')[0];
        if (titleScreenButton != null) titleScreenButton.Visible = !hideTitleScreenButtonOn.Contains(scene);
        if (restartRunButton != null) restartRunButton.Visible = !hiderestartRunButtonOn.Contains(scene);
    }
}
