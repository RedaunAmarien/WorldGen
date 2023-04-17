using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

[System.Serializable]
public class SubLocale
{
    [System.NonSerialized]
    public Locale parentLocale;
    public Vector2Int index;
    public string placeName;
    [Multiline]
    public string description;
    public Coordinates coordinates;
    public double avgElevation;
    public int timeZone;
    public List<Chunk> chunks;

    public Chunk GetChunk(Vector2Int index)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            Debug.LogFormat("Testing chunk {0} for {1}...", chunks[i].index, index);
            if (chunks[i].index == index)
            {
                Debug.LogFormat("Chunk {0} found.", index);
                return chunks[i];
            }
        }
        Debug.LogWarningFormat("Chunk {0} not found.", index);
        return null;
    }
}
