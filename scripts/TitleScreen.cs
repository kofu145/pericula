using Godot;

public partial class TitleScreen : Panel
{
    [Export] private Button startGameButton;

    // game config
    [Export] private RunConfig runConfig;
    [Export] private AnimationPlayer animation;

    public override void _Ready()
    {
        startGameButton.Pressed += StartNewRun;
        animation.Play("Start");
    }

    private void StartNewRun()
    {
        // reset globalManagers
        ResetManagers();
        startGameButton.Pressed -= StartNewRun;
        SceneManager.ChangeSceneToFile("Combat");
        SoundManager.PlaySE("click");
    }

    private void ResetManagers()
    {
        // deck and chips arent being disposed of
        ChipManager.Instance.StartNewRun(runConfig.playerStartingChips);
        StageManager.Instance.StartNewRun();
        DeckManager.Instance.StartNewRun(runConfig.playerStartingDeck);
        ShopManager.Instance.StartNewRun();
    }
}
