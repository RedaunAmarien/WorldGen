using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

[System.Serializable]
public class Locale
{
    [System.NonSerialized]
    public Region parentRegion;
    public string placeName;
    [Multiline]
    public string description;
    public Coordinates coordinates;
    public double avgElevation;
    public int timeZone;
    //public List<SubLocale> subLocales;

    //public SubLocale GetSubLocale(Vector2Int index)
    //{
    //    for (int i = 0; i < subLocales.Count; i++)
    //    {
    //        Debug.LogFormat("Testing chunk {0} for {1}...", subLocales[i].index, index);
    //        if (subLocales[i].index == index)
    //        {
    //            Debug.LogFormat("Chunk {0} found.", index);
    //            return subLocales[i];
    //        }
    //    }
    //    Debug.LogWarningFormat("Chunk {0} not found.", index);
    //    return null;
    //}
}
