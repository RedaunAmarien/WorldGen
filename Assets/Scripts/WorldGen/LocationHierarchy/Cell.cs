using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell : MonoBehaviour
{
    public Coordinates coordinates;
    public Vector2Int index;
    [System.NonSerialized]
    public Chunk parentChunk;
    [SerializeField] private List<Tile> baseTiles;
    [SerializeField] private List<GameObject> tileObjects;
    public GameObject cellPlane;

    public Cell(Vector2Int newIndex, Chunk parent)
    {
        index = newIndex;
        parentChunk = parent;
        baseTiles = new List<Tile>();
    }

    public void AddTile(Tile tile)
    {
        baseTiles.Add(tile);
    }

    public Tile GetTile(Vector2Int index)
    {
        for (int i = 0; i < baseTiles.Count; i++)
        {
            if (baseTiles[i].index == index)
                return baseTiles[i];
        }
        return null;
    }
}
