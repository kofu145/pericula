using Godot;

public partial class TitleScreen : Panel
{
    [Export] private Button startGameButton;
    [Export] private Button codexButton;

    // game config
    [Export] private RunConfig runConfig;
    [Export] private AnimationPlayer animation;

    public override void _Ready()
    {
        startGameButton.Pressed += StartNewRun;
        codexButton.Pressed += OpenCodex;
        animation.Play("Start");
    }

    private void StartNewRun()
    {
        // reset globalManagers
        RunManager.Instance.StartRun();
        startGameButton.Pressed -= StartNewRun;
        SoundManager.PlaySE("click");
    }

    private void OpenCodex()
    {
        SceneManager.ChangeSceneToFile("Codex");
        SoundManager.PlaySE("click");
    }

}
