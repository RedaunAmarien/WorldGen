using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Region
{
    public Vector2Int coordinates;
    [System.NonSerialized]
    public Quadrant parentQuadrant;
    public float[,] regionHeights;
    public float minHeight, maxHeight;
    public List<Locale> locales = new();
    public Region(Vector2Int location, Quadrant quad)
    {
        coordinates = location;
        parentQuadrant = quad;
    }
}