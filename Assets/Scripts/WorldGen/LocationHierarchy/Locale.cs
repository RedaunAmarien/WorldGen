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
}
