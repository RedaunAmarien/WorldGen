using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public Coordinates coordinates;
    public Vector2Int index;
    [System.NonSerialized]
    public Cell parentCell;
    public float elevation;

    public Tile(Vector2Int newIndex, Cell parent)
    {
        index = newIndex;
        parentCell = parent;
    }
}
