using Godot;

public partial class TitleScreen : Panel
{
    [Export] private Button startGameButton;
    [Export] private Button settingsButton;

    // game config
    [Export] private RunConfig runConfig;

    public override void _Ready()
    {
        startGameButton.Pressed += StartNewRun;
    }

    private void StartNewRun()
    {
        // reset globalManagers
        ResetManagers();
        // go to shop first
        SceneManager.ChangeSceneToFile("PreCombat");
        UiOverlay.Instance.Show();
    }

    private void ResetManagers()
    {
        ChipManager.Instance.StartNewRun(runConfig.playerStartingChips);
        StageManager.Instance.StartNewRun();
        DeckManager.Instance.StartNewRun(runConfig.playerStartingDeck);
    }
}
