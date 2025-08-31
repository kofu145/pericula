using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class DeathParam : Node
{
    public CardLane Lane;
    public int deadId;
    public CardData BaseData;
    public CardData? Reference;

    public void Initialize(CardLane lane, int id, CardData? unit)
    {
        Reference = unit;
        deadId = id;
        BaseData = Lookup.GetCardByID(id);
        Lane = lane;
    }

}
