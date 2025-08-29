using Godot;
using System;

[GlobalClass]
public partial class TutorialPage : Resource
{
    [Export] public Texture2D tutorialImage;
    [Export] public string pageName;
    [Export(PropertyHint.MultilineText)] public string pageDescription;
}
