using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quadrant
{
    public int index;
    [System.NonSerialized]
    public Planet pPlanet;
    public float minHeight, maxHeight;
    public Texture2D quadrantMapTex;
    public List<Region> regions = new List<Region>();

    public Quadrant(int i)
    {
        index = i;
    }
}
