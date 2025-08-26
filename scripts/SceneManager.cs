using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneManager : Control
{
    public static bool ChangingScenes = false;
    public static string CurrentScene;
    static SceneManager Instance;

    public override void _Ready()
    {
        Instance = this;
    }

    public static void ChangeSceneToFile(string target)
    {
        if (ResourceLoader.Exists(target))
        {
            Instance.ChangeSceneHelper(target);
        }
        else
        {
            GD.PrintErr($"Scene file '{target}' does not exist.");
        }
    }

    private void ChangeSceneHelper(string target)
    {
        GetTree().ChangeSceneToFile(target);
        CurrentScene = target;
    }
}
