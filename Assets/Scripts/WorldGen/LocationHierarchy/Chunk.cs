using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chunk
{
    public Vector2 longLatCoord;
    public Vector2Int index;
    [System.NonSerialized]
    public Locale pLocale;
    [SerializeField] private List<Cell> cells;

    public Chunk (Vector2Int newIndex, Locale parent)
    {
        index = newIndex;
        pLocale = parent;
        cells = new List<Cell>();
    }

    public void AddCell(Cell cell)
    {
        cells.Add(cell);
    }

    public Cell GetCell(Vector2Int index)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].index == index)
                return cells[i];
        }
        return null;
    }
}
