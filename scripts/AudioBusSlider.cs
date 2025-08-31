using Godot;
using System;

public partial class AudioBusSlider : Node
{
    [Export] string BusName;
    int BusIndex;

    public override void _Ready()
    {
        BusIndex = AudioServer.GetBusIndex(BusName);
        base._Ready();
    }

    public void OnValueChanged(float value)
    {
        AudioServer.SetBusVolumeDb(
            BusIndex,
            Mathf.LinearToDb(value)
        );
    }

    public void SoundFinishedChanging(bool value_changed)
    {
        SoundManager.PlaySE("select_node");
    }
}
