using Godot;
using System;

public partial class TutorialController : Panel
{
	[Export] private TextureRect texture;
	[Export] private Label header;
	[Export] private Label description;
	[Export] private Godot.Collections.Array<TutorialPage> tutorials;
	[Export] private Button nextPageButton;

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

		if (nextPageButton != null)
			nextPageButton.Pressed += OnNextPressed;
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

	private void InitializePage(int index)
	{
		var currentTutorial = tutorials[index];
		if (texture != null) texture.Texture = currentTutorial.tutorialImage;
		if (header != null) header.Text = currentTutorial.pageName;
		if (description != null) description.Text = currentTutorial.pageDescription;
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
