using UnityEngine;
using System.Collections;

public class Tile
{
    internal string Name;
    internal int AvoidValue;
    internal int MovementCost;
    internal int Defense;
    internal TileType Type;
    internal Vector2 Coordinate;

    public Tile()
    {
        MovementCost = 1;
    }
}
