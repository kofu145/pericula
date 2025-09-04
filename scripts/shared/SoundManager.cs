using Godot;
using System;

public partial class SoundManager : Node
{
    static AudioStreamPlayer BGMPlayer;
    static AudioStreamPlayer SEPlayer;


    public static void PlayBGM(string bgm)
    {
        if (GetCurrentBGMClip().Equals(bgm)) return;
        BGMPlayer.Set("parameters/switch_to_clip", bgm);

        if (!BGMPlayer.Playing)
        {
            BGMPlayer.Play();
        }
    }

    public static string GetCurrentBGMClip()
    {
        if (BGMPlayer != null)
        {
            // Assuming "parameters/switch_to_clip" holds the current clip name
            return (string)BGMPlayer.Get("parameters/switch_to_clip");
        }
        return null;
    }

    public void StopBGM()
    {

    }

    public static void PlaySE(string se, float pitch = 1)
    {
        AudioStreamPlaybackPolyphonic audio = (AudioStreamPlaybackPolyphonic)SEPlayer.GetStreamPlayback();
        audio.PlayStream(GD.Load<AudioStream>($"res://assets/audio/{se}.ogg"), 0, 0, pitch);
    }

    public override void _Ready()
    {
        BGMPlayer = GetNode<AudioStreamPlayer>("BGM");
        SEPlayer = GetNode<AudioStreamPlayer>("SE");
        SEPlayer.Play();
        BGMPlayer.Play();
    }
}
