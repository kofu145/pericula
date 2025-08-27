using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneManager : Node
{
	public static bool ChangingScenes = false;
	public static string CurrentScene;
	static SceneManager Instance;

	public override void _Ready()
	{
		Instance = this;
	}

	/// <summary>
	/// Switches to a new scene. Scene must be located in scenes folder.
	/// </summary>
	public static void ChangeSceneToFile(string target)
	{
		string absolutePath = $"scenes/{target}.tscn";
		if (ResourceLoader.Exists(absolutePath))
		{
			Instance.ChangeSceneHelper(absolutePath);
		}
		else
		{
			GD.PrintErr($"Scene file '{absolutePath}' does not exist.");
		}
	}

	private void ChangeSceneHelper(string target)
	{
		GetTree().ChangeSceneToFile(target);
		CurrentScene = target;
	}
}
