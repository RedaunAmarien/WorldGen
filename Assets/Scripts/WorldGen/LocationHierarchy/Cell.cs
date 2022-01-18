using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell
{
    public Vector2Int coordinates;
    [System.NonSerialized]
    public Chunk pChunk;
    public List<Tile> tiles = new List<Tile>();
}
