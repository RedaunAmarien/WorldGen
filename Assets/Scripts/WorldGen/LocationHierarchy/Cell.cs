using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell
{
    public Vector2 longLatCoord;
    public Vector2Int index;
    [System.NonSerialized]
    public Chunk pChunk;
    [SerializeField] private List<Tile> tiles;
    public GameObject cellPlane;

    public Cell(Vector2Int newIndex, Chunk parent)
    {
        index = newIndex;
        pChunk = parent;
        tiles = new List<Tile>();
    }

    public void AddTile(Tile tile)
    {
        tiles.Add(tile);
    }
}
