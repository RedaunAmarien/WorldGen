using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GlobeView : MonoBehaviour
{
    public GameObject planet;
    public GameObject planetParent;
    public GameObject sun;
    public GameObject sunParent;
    public float zoomSpeed;
    public float orbitSpeed;
    //[Tooltip("n.0 = Rising Equinox\nn.25 = Northern Solstice\nn.5 = Falling Equinox\nn.75 = Southern Solstice")]
    //public double yearProgress;
    //public float daysPerSecond;
    //public float sunRotX;
    //[Range(0,90)]
    //public float tropicsLatitude = 23.43647f;
    //public bool isOrtho = true;
    //public bool isRotating;
    // public Vector3 planetRotationSpeed;
    //float oneYear = 365.2524f * 360;
    //public TextMeshProUGUI dateText;
    Vector2 inputVal;
    float zoomVal;
    //public float minZoom = 0.1f;
    //public float maxZoom = 8;
    //public bool useGlenriCalendar;
    //public WorldTime currentTime;
    // public WorldTime startTime;

    //void Awake()
    //{
    //    if (useGlenriCalendar)
    //    {
    //        currentTime = new WorldTime(1, 1, 1, 0, 0, 0, 0);
    //        currentTime.monthsInYear = 8;
    //        currentTime.daysInMonth = new int[]
    //        {
    //            3, 89, 2, 89, 2, 89, 2, 89
    //        };
    //        currentTime.monthNames = new string[]
    //        {
    //            "Sherél Tazunio", "Sherél", "Serarél Tazunio", "Serarel",
    //            "Norél Tazunio", "Norél", "Jantorél Tazunio", "Jantorél"
    //        };
    //        currentTime.monthName = currentTime.monthNames[0];
    //        currentTime.hoursInDay = 25;
    //        currentTime.minutesInHour = 125;
    //        currentTime.secondsInMinute = 25;
    //        currentTime.leapMonth = 1;
    //    }
    //    else 
    //        currentTime = new WorldTime(1, 3, 20, 6, 0, 0, 0);
    //        // startTime = new WorldTime(1, 3, 20, 6, 0, 0, 0);
    //}

    void Update()
    {
        // Move view of whole system
        planetParent.transform.Rotate(new Vector3(0, orbitSpeed * Time.deltaTime * inputVal.x, 0), Space.Self);
        planetParent.transform.Rotate(new Vector3(orbitSpeed * Time.deltaTime * inputVal.y, 0, 0), Space.World);
        //planetParent.transform.localScale += Vector3.one * zoomSpeed * Time.deltaTime * zoomVal;
        //if (planetParent.transform.localScale.x < minZoom) planetParent.transform.localScale = Vector3.one * minZoom;
        //if (planetParent.transform.localScale.x > maxZoom) planetParent.transform.localScale = Vector3.one * maxZoom;

        // Set rotation value of planet for time-based shenanigans
        //if (isRotating) yearProgress += daysPerSecond / currentTime.daysInYear * Time.deltaTime;

        // Rotate Planet based on year
        //float planetRotY = (float)yearProgress * oneYear;
        //planet.transform.localRotation = Quaternion.Euler(0, -planetRotY, 0);

        // Offset Sun based on year
        //sunRotX = tropicsLatitude * Mathf.Sin((float)yearProgress*2*Mathf.PI);
        //sunParent.transform.localRotation = Quaternion.Euler(sunRotX, sunParent.transform.rotation.y, sunParent.transform.rotation.z);
    }

    //void LateUpdate()
    //{
    //    if (isRotating) currentTime.Add(daysPerSecond / currentTime.daysInYear * Time.deltaTime);
    //    dateText.text = string.Format("Date & Time at 0°N, 0°E:\n{0} {1}, Year {2:d4}\n<mspace=0.5em>{3:d2}:{4:d2}:{5:d2}.{6:d3}", currentTime.monthName, currentTime.day, currentTime.year,currentTime.hour, currentTime.minute, currentTime.second, Mathf.RoundToInt(currentTime.millisecond));

    //    // if (planetParent.transform.eulerAngles.z < -85) planetParent.transform.SetPositionAndRotation(planetParent.transform.position, Quaternion.Euler(planetParent.transform.rotation.eulerAngles.x, planetParent.transform.rotation.eulerAngles.y, -85));
    //    // if (planetParent.transform.eulerAngles.z > 85) planetParent.transform.SetPositionAndRotation(planetParent.transform.position, Quaternion.Euler(planetParent.transform.rotation.eulerAngles.x, planetParent.transform.rotation.eulerAngles.y, 85));
    //    // if (planetParent.transform.eulerAngles.x < -85) planetParent.transform.SetPositionAndRotation(planetParent.transform.position, Quaternion.Euler(-85, planetParent.transform.rotation.eulerAngles.y, planetParent.transform.rotation.eulerAngles.z));
    //    // if (planetParent.transform.eulerAngles.x > 85) planetParent.transform.SetPositionAndRotation(planetParent.transform.position, Quaternion.Euler(85, planetParent.transform.rotation.eulerAngles.y, planetParent.transform.rotation.eulerAngles.z));
    //}

    void OnOrbit(InputValue value)
    {
        inputVal = value.Get<Vector2>();
    }

    void OnZoom(InputValue value)
    {
        zoomVal = value.Get<float>();
    }
}
