using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocalePin : MonoBehaviour
{
    public Locale linkedLocale;
    GlobeView globeView;
    //WorldManager worldManager;
    TextMeshPro label;
    WorldTime localTime;

    void Start()
    {
        globeView = GameObject.Find("PlanetSystem").GetComponent<GlobeView>();
        //worldManager = GameObject.Find("PlanetSystem").GetComponent<WorldManager>();
        // timeZone = Mathf.RoundToInt(longLatCoord.x/360*globeView.currentTime.hoursInDay);
        label = GetComponentInChildren<TextMeshPro>();
        localTime = new WorldTime(globeView.currentTime);
        localTime.AddHours(linkedLocale.timeZone);
    }

    void LateUpdate()
    {
        if (globeView.isRotating)
        {
            localTime.doubleYears = globeView.currentTime.doubleYears;
            localTime.AddHours(linkedLocale.timeZone); 
        }
        label.text = linkedLocale.placeName;
        // label.text = string.Format("{9}\n{0:d2}:{6:d2}\n{1:n2}°{2}, {3:n2}°{4}\n{7}\n{8}\n{5}", localTime.hour, Mathf.Abs(linkedLocale.longLatCoord.y), linkedLocale.longLatCoord.y > 0 ? "N" : "S", Mathf.Abs(linkedLocale.longLatCoord.x), linkedLocale.longLatCoord.x > 0 ? "E" : "W", worldManager.useMiles ? linkedLocale.avgElevation * 3.28084f + "'" : linkedLocale.avgElevation + " m", localTime.minute, linkedLocale.xyzCoord.ToString(), linkedLocale.uvqCoord.ToString(), linkedLocale.placeName);
    }

    public void ChooseLocation()
    {
        GameRam.currentLocale = linkedLocale;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Location");
    }
}
