using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

[System.Serializable]
public class Locale
{
    [System.NonSerialized]
    public Region pRegion;
    public string placeName;
    public string description;
    public Vector3Int uvqCoord;
    public Vector2 longLatCoord;
    public Vector3 xyzCoord;
    public double avgElevation;
    public int timeZone;
    // public enum Biome
    // {
    //     Tropical, Temperate, Taiga, Tundra, Highland
    // };
    // public Biome biome;
    // public Vector2 baseTempRange;
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
