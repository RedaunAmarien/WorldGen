using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell
{
    public Vector2 longLatCoord;
    [System.NonSerialized]
    public Chunk pChunk;
    public List<Tile> tiles;
}
