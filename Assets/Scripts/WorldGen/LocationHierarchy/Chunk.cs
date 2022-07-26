using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chunk
{
    public Vector2 longLatCoord;
    [System.NonSerialized]
    public Locale pLocale;
    public List<Cell> cells;
}
