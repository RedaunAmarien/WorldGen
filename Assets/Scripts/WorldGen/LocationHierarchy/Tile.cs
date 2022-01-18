using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public Vector2Int coordinates;
    [System.NonSerialized]
    public Cell pCell;
    public float elevation;
}
