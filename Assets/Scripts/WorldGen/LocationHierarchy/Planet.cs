using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Planet
{
    [System.NonSerialized]
    public Generator generator;
    public Vector2Int worldMapSize;
    public Vector2 worldMapCenter;
    [Min(0)]
    public float worldMapZoom;

    [Header("Generation Settings")]
    public int seed;
    public bool useCustomSeed;
    public Generator.NoiseType noiseType;
    public Generator.FractalType fractalType;
    [Min(0.000001f)]
    public float noiseFrequency = 1f;
    [Range(0,1)]
    public float noiseStrength;
    public bool isMode2D;
    [Min(1)]
    public float globeScale;
    public Vector3 globeOffset;
    public bool lockOffset;
    public float minHeight, maxHeight;
    public int minElevation = -10000;
    public int maxElevation = 10000;
    // [Range(0,1)]
    // public float waterlevel = 0.5f;

    [Header("Octave Settings")]
    public float lacunarity = 2;
    public float persistence = 0.5f;
    [Min(1)]
    public int totalOctaves = 5;
    public List<Quadrant> quadrants = new List<Quadrant>();

    // public PlanetCompressed Compress()
    // {
    //     PlanetCompressed plan = new PlanetCompressed();
    //     plan.quadrants = quadrants;
    //     plan.worldSeed = seed;
    //     plan.totalOctaves = totalOctaves;
    //     plan.persistence = persistence;
    //     plan.noiseType = noiseType;
    //     plan.noiseStrength = noiseStrength;
    //     plan.noiseFrequency = noiseFrequency;
    //     plan.noise = noise;
    //     plan.minElevation = minElevation;
    //     plan.maxElevation = maxElevation;
    //     plan.mapMinMax = mapMinMax;
    //     plan.lacunarity = lacunarity;
    //     plan.isMode2D = isMode2D;
    //     plan.globeScale = globeScale;
    //     plan.globeOffset = globeOffset;
    //     plan.fractalType = fractalType;

    //     return plan;
    // }

    // public void Decompress(PlanetCompressed plan)
    // {
    //     seed = plan.worldSeed;
    //     totalOctaves = plan.totalOctaves;
    //     persistence = plan.persistence;
    //     noiseType = plan.noiseType;
    //     noiseStrength = plan.noiseStrength;
    //     noiseFrequency = plan.noiseFrequency;
    //     noise = plan.noise;
    //     minElevation = plan.minElevation;
    //     maxElevation = plan.maxElevation;
    //     mapMinMax = plan.mapMinMax;
    //     lacunarity = plan.lacunarity;
    //     isMode2D = plan.isMode2D;
    //     globeScale = plan.globeScale;
    //     globeOffset = plan.globeOffset;
    //     fractalType = plan.fractalType;
    //     lockSeed = true;
    //     lockOffset = true;
    // }
}
