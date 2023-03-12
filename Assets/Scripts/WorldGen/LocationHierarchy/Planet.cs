using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Planet
{
    [System.NonSerialized]
    public WorldManager worldManager;

    [Header("Generation Settings")]
    [Min(1)]
    public float globeScale;
    public float minHeight;
    public float maxHeight;
    public float lowestElevation = -10000;
    public float highestElevation = 10000;
    [Range(0,1)]
    public double waterlevel = 0.5f;
    public List<Quadrant> quadrants = new();
}
