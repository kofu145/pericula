using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public partial class BugReport : Control
{
    // UI refs
    [Export] private TextEdit bugDescription;
    [Export] private HSlider severity;

    [Export] private PackedScene reponsePanel;

    // report form
    private string formUrl = "https://docs.google.com/forms/d/e/1FAIpQLSdofjDayaVyXOm9NsmbwpOp1S7ZWsBsDXFD5g0JGDW9PPmTVg/formResponse";
    private string bugEntryId = "entry.1413594413";
    private string severityEntryId = "entry.1028849403";

    public override void _Ready()
    {
        Visible = false;
    }

    public void Close()
    {
        Visible = false;
    }

    public void ButtonClick()
    {
        if (string.IsNullOrEmpty(bugDescription.Text)) return;
        SubmitBugReport(bugDescription.Text);
    }

    public async Task SubmitBugReport(string report)
    {
        if (string.IsNullOrEmpty(report))
        {
            GD.PushWarning("Bug report is empty. Please provide a report before submitting.");
            return;
        }

        var httpRequest = new HttpRequest();
        AddChild(httpRequest);

        string body =
            $"{Uri.EscapeDataString(bugEntryId)}={Uri.EscapeDataString(report)}" +
            $"&{Uri.EscapeDataString(severityEntryId)}={Uri.EscapeDataString(severity.Value.ToString())}";

        string[] headers = new[]
        {
            "Content-Type: application/x-www-form-urlencoded"
        };

        Error err = httpRequest.Request(
            formUrl,
            headers,
            Godot.HttpClient.Method.Post,
            body
        );

        if (err != Error.Ok)
        {
            GD.PushWarning($"Failed to start request: {err}");
            InitializeResponse(false);
            httpRequest.QueueFree();
            return;
        }

        // Wait for the signal and unpack/cast the args
        var raw = await ToSignal(httpRequest, HttpRequest.SignalName.RequestCompleted);
        long result = (long)raw[0];
        long responseCode = (long)raw[1];

        // Optional: decode body to string for debugging

        GD.Print($"Result: {result}, Code: {responseCode}");

        if (responseCode == 200)
        {
            GD.Print("Bug report submitted successfully.");
            InitializeResponse(true);
        }
        else
        {
            GD.PushWarning($"Failed with HTTP {responseCode}");
            InitializeResponse(false);
        }
        httpRequest.QueueFree();
    }

    // helpers
    private void InitializeResponse(bool isSuccess)
    {
        Reset();
        var panel = reponsePanel.Instantiate<BugReportResponse>();
        panel.Initialize(isSuccess);
        GetTree().Root.AddChild(panel);
    }
    private void Reset()
    {
        bugDescription.Text = "";
        Visible = false;
    }
}
