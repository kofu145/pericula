using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;

public partial class Settings : Panel
{
    // UI refs
    [Export] private Button titleScreenButton;
    [Export] private Button restartRunButton;
    [Export] private Godot.Collections.Array<string> hideTitleScreenButtonOn;
    [Export] private Godot.Collections.Array<string> hiderestartRunButtonOn;
    // report form
    private string formUrl = "https://docs.google.com/forms/d/1uW1gZqDA6q-Ujmqwc_kVSTrZm0VpNmajWwDCQHfETjU/formResponse";
    private string bugEntryId = "entry.1413594413";
    // private string severityEntryId = "entry.1028849403_sentinel";


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

    // public void SubmitBugReport(string report)
    // {
    //     if (string.IsNullOrEmpty(report))
    //     {
    //         GD.PushWarning("Bug report is empty. Please provide a report before submitting.");
    //         return;
    //     }

    //     if (Send(report).IsCompletedSuccessfully)
    //     {
    //         GD.Print("Bug report submitted successfully.");
    //         // update text to show bug has been reported
    //     }
    //     else
    //     {
    //         GD.PushWarning("Failed to submit bug report.");
    //     }
    // }

    // private async System.Threading.Tasks.Task<bool> Send(string report)
    // {
    //     var formData = new Dictionary<string, string>()
    //     {
    //         { bugEntryId, report},
    //         // { severityEntryId, selectedSeverity.ToString() },
    //     };

    //     try
    //     {
    //         using (Godot.HttpClient httpClient = new Godot.HttpClient())
    //         {
    //             var content = new FormUrlEncodedContent(formData);

    //             HttpResponseMessage response = await httpClient.PostAsync(formUrl, content);
    //             if (response.IsSuccessStatusCode)
    //             {
    //                 GD.Print("Bug report submitted successfully.");
    //                 return true;
    //             }
    //             else
    //             {
    //                 GD.PushWarning($"Failed to submit bug report. Status code: {response.StatusCode}");
    //                 return false;
    //             }
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         GD.PushWarning($"Exception occurred while submitting bug report: {e.Message}");
    //         return false;
    //     }
    // }
}
