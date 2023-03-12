using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell
{
    public Coordinates coordinates;
    //public Vector2 longLatCoord;
    public Vector2Int index;
    [System.NonSerialized]
    public Chunk parentChunk;
    [SerializeField] private List<Tile> tiles;
    public GameObject cellPlane;

    public Cell(Vector2Int newIndex, Chunk parent)
    {
        index = newIndex;
        parentChunk = parent;
        tiles = new List<Tile>();
    }

    public void AddTile(Tile tile)
    {
        tiles.Add(tile);
    }

    public Tile GetTile(Vector2Int index)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].index == index)
                return tiles[i];
        }
        return null;
    }
}
