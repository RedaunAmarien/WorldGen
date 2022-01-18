using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public static class GameRam
{
    public static Planet planet;

    public static SaveData ToSaveData()
    {
        SaveData newData = new SaveData();
        newData.planet = planet;

        return newData;
    }

    public static void FromSaveData(SaveData data)
    {
        planet = data.planet;
        planet.useCustomSeed = true;

        GameObject.Find("PlanetSystem").GetComponent<Generator>().LoadedPlanet(planet);
    }
}
