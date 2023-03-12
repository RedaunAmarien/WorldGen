using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quadrant
{
    public int index;
    [System.NonSerialized]
    public Planet parentPlanet;
    public float minHeight;
    public float maxHeight;
    public Texture2D quadrantMapTex;
    public List<Region> regions = new();

    public Quadrant(int i)
    {
        index = i;
    }
}
