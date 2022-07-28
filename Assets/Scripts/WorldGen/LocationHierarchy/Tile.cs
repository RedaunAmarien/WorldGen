using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public Vector2 longLatCoord;
    public Vector3 subUvqCoord;
    public Vector2Int index;
    [System.NonSerialized]
    public Cell pCell;
    public float elevation;

    public Tile(Vector2Int newIndex, Cell parent)
    {
        index = newIndex;
        pCell = parent;
    }
}
