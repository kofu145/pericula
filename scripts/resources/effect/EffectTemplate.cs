using Godot;
using System;

[GlobalClass]
public partial class EffectTemplate : Resource
{

    [Export] public string name;
    public virtual void OnUse(EffectParam param) { }

    public virtual void Initialize(EffectParam param) { }
}
