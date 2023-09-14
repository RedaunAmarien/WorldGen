using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chunk
{
    public Coordinates coordinates;
    public Vector2Int index;
    public int cellsPerChunkEdge = 8;
    [SerializeField] private Cell[,] cells;

    public Chunk(Vector2Int newIndex)
    {
        index = newIndex;
        cells = new Cell[cellsPerChunkEdge, cellsPerChunkEdge];
    }

    public void SetCell(Cell cell)
    {
        cells[cell.index.x, cell.index.y] = cell;
    }

    public Cell GetCell(Vector2Int index)
    {
        return cells[index.x, index.y];
    }
}
