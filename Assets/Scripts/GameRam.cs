using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public static class GameRam
{
    public static Planet planet;
    public static Locale currentLocale;
    public static class NoiseSettings
    {
        public static int mSeed;
        public static float mFrequency;
        public static FastNoiseLite.FractalType mFractalType;
        public static FastNoiseLite.NoiseType mNoiseType;
        public static int mOctaves;
        public static float mLacunarity;
        public static float mGain;
    }

    public static SaveData ToSaveData()
    {
        SaveData newData = new SaveData();
        newData.planetSeed = NoiseSettings.mSeed;
        newData.noiseFreq = NoiseSettings.mFrequency;
        newData.fractalType = NoiseSettings.mFractalType;
        newData.noiseType = NoiseSettings.mNoiseType;
        newData.lacunarity = NoiseSettings.mLacunarity;
        newData.octaves = NoiseSettings.mOctaves;
        newData.gain = NoiseSettings.mGain;

        return newData;
    }

    public static void FromSaveData(SaveData data)
    {
        NoiseSettings.mSeed = data.planetSeed;
        NoiseSettings.mFrequency = data.noiseFreq;
        NoiseSettings.mFractalType = data.fractalType;
        NoiseSettings.mNoiseType = data.noiseType;
        NoiseSettings.mLacunarity = data.lacunarity;
        NoiseSettings.mOctaves = data.octaves;
        NoiseSettings.mGain = data.gain;

        GameObject.Find("World Manager").GetComponent<WorldManager>().LoadedPlanet(planet);
    }
}
