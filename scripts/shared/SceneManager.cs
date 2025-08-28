using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneManager : Node
{
	[Export] ColorRect SceneTransitionAnimationRect;
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

	private async void ChangeSceneHelper(string target)
	{
		SceneTransitionAnimationRect.MouseFilter = Control.MouseFilterEnum.Stop;

		var TweenFade = CreateTween();
		TweenFade.SetParallel(true);
		TweenFade.TweenProperty(
			SceneTransitionAnimationRect.Material,
			"shader_parameter/progress",
			0.7,
			0.5f
		).From(0f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);

		await ToSignal(TweenFade, "finished");

		GetTree().ChangeSceneToFile(target);
		CurrentScene = target;
		
		var TweenUnfade = CreateTween();
		TweenUnfade.TweenProperty(
			SceneTransitionAnimationRect.Material,
			"shader_parameter/progress",
			0,
			0.75f
		).From(0.75f).SetTrans(Tween.TransitionType.Sine);

		SceneTransitionAnimationRect.MouseFilter = Control.MouseFilterEnum.Ignore;
		GetTree().ChangeSceneToFile(target);

		await ToSignal(TweenUnfade, "finished");
		ChangingScenes = false;
	}
}
