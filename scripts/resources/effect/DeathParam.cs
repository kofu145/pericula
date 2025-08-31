using Godot;
using System.Threading.Tasks;
using System;

[GlobalClass]
public partial class DeathParam : Node
{
    public CardLane Lane;
    public int deadId;
    public CardData BaseData;

    public void Initialize(CardLane lane, int id)
    {
        deadId = id;
        BaseData = Lookup.GetCardByID(id);
        Lane = lane;
    }

}
