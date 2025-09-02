using Godot;
using System;

public partial class TutorialController : CanvasLayer
{
    // page info
    [Export] private TextureRect texture;
    [Export] private Label header;
    [Export] private Label description;

    // page number
    [Export] private Godot.Collections.Array<TutorialPage> tutorials;
    [Export] private Button backPageButton;
    [Export] private Button nextPageButton;
    [Export] private Label TutorialPageNumber;

    [Export] private string tutorialKey = "CombatTutorial";

    // For testing
    [Export] private bool overrideTutorialOnlyOnce = false;

    private int currentPage = 0;

    private const string SavePath = "user://settings.cfg";
    private const string Section = "Tutorial";

    public override void _Ready()
    {
        Visible = true;
        // Skip if already completed
        if (IsTutorialCompleted() && !overrideTutorialOnlyOnce)
        {
            QueueFree();
            return;
        }

        if (tutorials.Count > 0)
            InitializePage(currentPage);

        if (nextPageButton != null) nextPageButton.Pressed += OnNextPressed;
        if (backPageButton != null) backPageButton.Pressed += OnBackPressed;
    }

    private void OnNextPressed()
    {
        currentPage++;
        if (currentPage >= tutorials.Count)
        {
            FinishTutorial();
            return;
        }

        InitializePage(currentPage);
    }

    private void OnBackPressed()
    {
        currentPage--;
        InitializePage(currentPage);
    }

    private void InitializePage(int index)
    {
        if (index < 0 || index == tutorials.Count) return;  // abort if index out of bounds

        var currentTutorial = tutorials[index];
        if (texture != null) texture.Texture = currentTutorial.tutorialImage;
        if (header != null) header.Text = currentTutorial.pageName;
        if (description != null) description.Text = currentTutorial.pageDescription;

        if (TutorialPageNumber != null) TutorialPageNumber.Text = $"Tutorial: {index + 1} / {tutorials.Count}";
        if (backPageButton != null) backPageButton.Visible = index != 0;
        if (nextPageButton != null) nextPageButton.Text = index == tutorials.Count - 1 ? "Complete" : "Next";
    }

    private void FinishTutorial()
    {
        SetTutorialCompleted();
        QueueFree();
    }

    private bool IsTutorialCompleted()
    {
        var cfg = new ConfigFile();
        // If no file yet, not completed
        if (cfg.Load(SavePath) != Error.Ok) return false;

        return (bool)cfg.GetValue(Section, tutorialKey, false);
    }

    private void SetTutorialCompleted()
    {
        var cfg = new ConfigFile();
        cfg.Load(SavePath);
        cfg.SetValue(Section, tutorialKey, true);
        cfg.Save(SavePath);
    }
}
