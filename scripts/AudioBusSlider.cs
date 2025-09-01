using Godot;
using System;

public partial class AudioBusSlider : Node
{
    private const string SavePath = "user://settings.cfg";
    private const string Section = "Audio";
    [Export] string BusName;
    [Export] Slider slider;
    int BusIndex;

    public override void _Ready()
    {
        BusIndex = AudioServer.GetBusIndex(BusName);
        base._Ready();

        var cfg = new ConfigFile();
        if (cfg.Load(SavePath) == Error.Ok)
        {
            slider.Value = (float)cfg.GetValue(Section, BusName, 1f);
            OnValueChanged((float)slider.Value);
        }
    }

    public void OnValueChanged(float value)
    {
        AudioServer.SetBusVolumeDb(
            BusIndex,
            Mathf.LinearToDb(value)
        );

        var cfg = new ConfigFile();
        cfg.Load(SavePath); 
        cfg.SetValue(Section, BusName, value);
        cfg.Save(SavePath);
    }

    public void SoundFinishedChanging(bool value_changed)
    {
        SoundManager.PlaySE("select_node");
    }
}
