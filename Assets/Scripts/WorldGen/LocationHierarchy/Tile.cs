using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public Vector2 longLatCoord;
    public Vector3 subUvqCoord;
    [System.NonSerialized]
    public Cell pCell;
    public float elevation;
}
