using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GlobeView : MonoBehaviour
{
    public GameObject planet, planetParent, sun, sunParent;
    public float zoomSpeed, orbitSpeed;
    [Tooltip("n.0 = Rising Equinox\nn.25 = Northern Solstice\nn.5 = Falling Equinox\nn.75 = Southern Solstice")]
    public double yearProgress;
    public float daysPerSecond;
    public float sunRotX;
    [Range(0,90)]
    public float tropicsLatitude = 23.43647f;
    public bool isOrtho = true;
    public bool isRotating;
    // public Vector3 planetRotationSpeed;
    float oneYear = 365.2524f * 360;
    public TextMeshProUGUI dateText;
    Vector2 inputVal;
    float zoomVal;
    //public float minZoom = 0.1f;
    //public float maxZoom = 8;
    public bool useGlenriCalendar;
    public WorldTime currentTime;
    // public WorldTime startTime;

    void Awake()
    {
        if (useGlenriCalendar)
        {
            currentTime = new WorldTime(1, 1, 1, 0, 0, 0, 0);
            currentTime.monthsInYear = 8;
            currentTime.daysInMonth = new int[]
            {
                3, 89, 2, 89, 2, 89, 2, 89
            };
            currentTime.monthNames = new string[]
            {
                "Sherél Tazunio", "Sherél", "Serarél Tazunio", "Serarel",
                "Norél Tazunio", "Norél", "Jantorél Tazunio", "Jantorél"
            };
            currentTime.monthName = currentTime.monthNames[0];
            currentTime.hoursInDay = 25;
            currentTime.minutesInHour = 125;
            currentTime.secondsInMinute = 25;
            currentTime.leapMonth = 1;
        }
        else 
            currentTime = new WorldTime(1, 3, 20, 6, 0, 0, 0);
            // startTime = new WorldTime(1, 3, 20, 6, 0, 0, 0);
    }

    void Update()
    {
        // Move view of whole system
        planetParent.transform.Rotate(new Vector3(0, orbitSpeed * Time.deltaTime * inputVal.x, 0), Space.Self);
        planetParent.transform.Rotate(new Vector3(orbitSpeed * Time.deltaTime * inputVal.y, 0, 0), Space.World);
        //planetParent.transform.localScale += Vector3.one * zoomSpeed * Time.deltaTime * zoomVal;
        //if (planetParent.transform.localScale.x < minZoom) planetParent.transform.localScale = Vector3.one * minZoom;
        //if (planetParent.transform.localScale.x > maxZoom) planetParent.transform.localScale = Vector3.one * maxZoom;

        // Set rotation value of planet for time-based shenanigans
        if (isRotating) yearProgress += daysPerSecond / currentTime.daysInYear * Time.deltaTime;

        // Rotate Planet based on year
        float planetRotY = (float)yearProgress * oneYear;
        planet.transform.localRotation = Quaternion.Euler(0, -planetRotY, 0);
        
        // Offset Sun based on year
        //sunRotX = tropicsLatitude * Mathf.Sin((float)yearProgress*2*Mathf.PI);
        //sunParent.transform.localRotation = Quaternion.Euler(sunRotX, sunParent.transform.rotation.y, sunParent.transform.rotation.z);
    }

    void LateUpdate()
    {
        if (isRotating) currentTime.Add(daysPerSecond / currentTime.daysInYear * Time.deltaTime);
        dateText.text = string.Format("Date & Time at 0°N, 0°E:\n{0} {1}, Year {2:d4}\n<mspace=0.5em>{3:d2}:{4:d2}:{5:d2}.{6:d3}", currentTime.monthName, currentTime.day, currentTime.year,currentTime.hour, currentTime.minute, currentTime.second, Mathf.RoundToInt(currentTime.millisecond));

        // if (planetParent.transform.eulerAngles.z < -85) planetParent.transform.SetPositionAndRotation(planetParent.transform.position, Quaternion.Euler(planetParent.transform.rotation.eulerAngles.x, planetParent.transform.rotation.eulerAngles.y, -85));
        // if (planetParent.transform.eulerAngles.z > 85) planetParent.transform.SetPositionAndRotation(planetParent.transform.position, Quaternion.Euler(planetParent.transform.rotation.eulerAngles.x, planetParent.transform.rotation.eulerAngles.y, 85));
        // if (planetParent.transform.eulerAngles.x < -85) planetParent.transform.SetPositionAndRotation(planetParent.transform.position, Quaternion.Euler(-85, planetParent.transform.rotation.eulerAngles.y, planetParent.transform.rotation.eulerAngles.z));
        // if (planetParent.transform.eulerAngles.x > 85) planetParent.transform.SetPositionAndRotation(planetParent.transform.position, Quaternion.Euler(85, planetParent.transform.rotation.eulerAngles.y, planetParent.transform.rotation.eulerAngles.z));
    }

    void OnOrbit(InputValue value)
    {
        inputVal = value.Get<Vector2>();
    }

    void OnZoom(InputValue value)
    {
        zoomVal = value.Get<float>();
    }
}

[System.Serializable]
public class WorldTime
{
    // Default date to 01/01/0001 00:00:00.0
    public int year = 1;
    public int month = 1;
    public int day = 1;
    public int dayOfYear = 1;
    public int hour = 0;
    public int minute = 0;
    public int second = 0;
    public float millisecond = 0;

    // Default fractionals to Earth-like
    public int monthsInYear = 12;
    public string[] monthNames =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };
    public string monthName = "January";
    // daysInYear does not include leap days; they are handled separately.
    public int daysInYear = 365;
    public int[] daysInMonth =
    {
        31, 28, 31, 30, 31, 30, 31,
        31, 30, 31, 30, 31
    };
    public int hoursInDay = 24;
    public int minutesInHour = 60;
    public int secondsInMinute = 60;
    public int millisecondsInSecond = 1000;

    public int leapMonth = 2;
    public int leapDays = 1;
    public int leapFrequency = 4;

    public double doubleYears = 0;

    public WorldTime(WorldTime time)
    {
        Simplify(time.doubleYears);
    }

    public WorldTime(int y, int mo, int d)
    {
        year = y;
        doubleYears += year;
        month = mo;
        monthName = monthNames[mo-1];
        // doubleYears += (float)month/monthsInYear;
        day = d;
        dayOfYear = 0;
        for (int i = 0; i < month-1; i++)
        {
            dayOfYear += daysInMonth[i];
            if (year % leapFrequency == 0 && i + 1 == leapMonth)
            {
                dayOfYear += leapDays;
            }
        }
        dayOfYear += day;
        doubleYears += (float)dayOfYear/daysInYear;
    }

    public WorldTime(int h, int mi, int s, float mil)
    {
        doubleYears += year;
        // doubleYears += (float)month/monthsInYear;
        doubleYears += (float)dayOfYear/daysInYear;

        hour = h;
        doubleYears += (float)hour/hoursInDay/daysInYear;
        minute = mi;
        doubleYears += (float)minute/minutesInHour/hoursInDay/daysInYear;
        second = s;
        doubleYears += (float)second/secondsInMinute/minutesInHour/hoursInDay/daysInYear;
        millisecond = mil;
        doubleYears += (float)millisecond/millisecondsInSecond/secondsInMinute/minutesInHour/hoursInDay/daysInYear;
    }
    public WorldTime(int y, int mo, int d, int h, int mi, int s, float mil)
    {
        year = y;
        doubleYears += year;
        month = mo;
        monthName = monthNames[mo-1];
        // doubleYears += (float)month/monthsInYear;
        day = d;
        dayOfYear = 0;
        for (int i = 0; i < mo-1; i++)
        {
            dayOfYear += daysInMonth[i];
            if (year % leapFrequency == 0 && i + 1 == leapMonth)
            {
                dayOfYear += leapDays;
            }
        }
        dayOfYear += day;
        doubleYears += (float)dayOfYear/daysInYear;

        hour = h;
        doubleYears += (float)hour/hoursInDay/daysInYear;
        minute = mi;
        doubleYears += (float)minute/minutesInHour/hoursInDay/daysInYear;
        second = s;
        doubleYears += (float)second/secondsInMinute/minutesInHour/hoursInDay/daysInYear;
        millisecond = mil;
        doubleYears += millisecond/millisecondsInSecond/secondsInMinute/minutesInHour/hoursInDay/daysInYear;
    }

    public void Add(double years)
    {
        Simplify(doubleYears + years);
    }

    public void AddDays(float days)
    {
        days = days / daysInYear;
        Simplify(doubleYears + days);
    }

    public void AddHours(float hours)
    {
        hours = hours / hoursInDay / daysInYear;
        Simplify(doubleYears + hours);
    }

    void Simplify(double totalValue)
    {
        double tempTotal =  totalValue;
        doubleYears = totalValue;

        year = (int)System.Math.Floor(tempTotal);
        tempTotal -= year;

        int tempDaysInYear = daysInYear;
        if (year % leapFrequency == 0)
        {
            tempDaysInYear += leapDays;
        }

        tempTotal *= tempDaysInYear;
        dayOfYear = (int)System.Math.Floor(tempTotal);
        tempTotal -= dayOfYear;

        tempTotal *= hoursInDay;
        hour = (int)System.Math.Floor(tempTotal);
        tempTotal -= hour;

        tempTotal *= minutesInHour;
        minute = (int)System.Math.Floor(tempTotal);
        tempTotal -= minute;
        
        tempTotal *= secondsInMinute;
        second = (int)System.Math.Floor(tempTotal);
        tempTotal -= second;

        tempTotal *= millisecondsInSecond;
        millisecond = (float)tempTotal;

        month = 1;
        day = dayOfYear;
        for (int i = 0; i < daysInMonth.Length; i++)
        {
            if (day > daysInMonth[i])
            {
                month ++;
                int tempDaysInMonth = daysInMonth[i];
                if (year % leapFrequency == 0 && i + 1 == leapMonth)
                {
                    tempDaysInMonth += leapDays;
                }
                day -= tempDaysInMonth;
            }
            else break;
        }
        monthName = monthNames[month-1];
    }
}
