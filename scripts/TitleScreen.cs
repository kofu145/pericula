using Godot;

public partial class TitleScreen : Panel
{
    [Export] private Button startGameButton;

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
        startGameButton.Pressed -= StartNewRun;
        SceneManager.ChangeSceneToFile("PreCombat");
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
