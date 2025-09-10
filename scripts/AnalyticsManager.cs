using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public partial class AnalyticsManager : Node
{
    public static AnalyticsManager Instance;

    private string WebhookBaseUrl = "https://script.google.com/macros/s/AKfycbwDOOfz3wVj672beBuc0uJS1eFtD4DPpScH7Qj950AXoRX14jIOWI-fxFT_FqeAH_b-FQ/exec";
    private string Secret = "V3dbvTJil3";

    public override void _Ready()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }
    }

    public async void LogEnemyBattle(string enemyId, string enemyName, bool playerWon, string clientVersion = "dev")
    {
        var payload = new EnemyEvent
        {
            event_type = "enemy_battle",
            timestamp = DateTime.UtcNow.ToString("o"),
            client_version = clientVersion,
            enemy_id = enemyId,
            enemy_name = enemyName,
            win_count = playerWon ? 0 : 1,
            loss_count = playerWon ? 1 : 0
        };

        await Post(JsonSerializer.Serialize(payload));
    }

    public async void LogRun(Dictionary<int, int> deckCounts, bool playerWon, string enemyId = "", string clientVersion = "dev")
    {
        var payload = new RunEvent
        {
            event_type = "card_run",
            client_version = clientVersion,
            deck_counts = deckCounts,
            win = playerWon ? 1 : 0,
            loss = playerWon ? 0 : 1,
            enemy_id = enemyId
        };

        await Post(JsonSerializer.Serialize(payload));
    }

    // model
    private class EnemyEvent
    {
        public string event_type { get; set; }
        public string timestamp { get; set; }
        public string client_version { get; set; }
        public string enemy_id { get; set; }
        public string enemy_name { get; set; }
        public int win_count { get; set; }
        public int loss_count { get; set; }
    }

    private class RunEvent
    {
        public string event_type { get; set; }
        public string client_version { get; set; }
        public Dictionary<int, int> deck_counts { get; set; }
        public int win { get; set; }
        public int loss { get; set; }
        public string enemy_id { get; set; }
    }

    private async Task Post(string json)
    {
        GD.Print(json);
        var httpRequest = new HttpRequest();
        AddChild(httpRequest);

        var url = $"{WebhookBaseUrl}?secret={Uri.EscapeDataString(Secret)}";

        string[] headers = { "Content-Type: application/plain" };

        Error err = httpRequest.Request(
                    url,
                    headers,
                    HttpClient.Method.Post,
                    json
                );

        if (err != Error.Ok)
        {
            GD.PushWarning($"Failed to start request: {err}");
            httpRequest.QueueFree();
            return;
        }

        // Wait for the signal and unpack/cast the args
        var raw = await ToSignal(httpRequest, HttpRequest.SignalName.RequestCompleted);
        long result = (long)raw[0];
        long responseCode = (long)raw[1];

        GD.Print($"Result: {result}, Code: {responseCode}");
        httpRequest.QueueFree();
    }

}
