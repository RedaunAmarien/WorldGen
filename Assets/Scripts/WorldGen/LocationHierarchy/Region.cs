using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Region
{
    public Vector2Int coordinates;
    [System.NonSerialized]
    public Quadrant pQuadrant;
    public float[,] regionHeights;
    public float minHeight, maxHeight;
    public List<Locale> locales = new List<Locale>();
    public Region(Vector2Int loc, Quadrant quad)
    {
        coordinates = loc;
        pQuadrant = quad;
    }
}