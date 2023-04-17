using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        monthName = monthNames[mo - 1];
        // doubleYears += (float)month/monthsInYear;
        day = d;
        dayOfYear = 0;
        for (int i = 0; i < month - 1; i++)
        {
            dayOfYear += daysInMonth[i];
            if (year % leapFrequency == 0 && i + 1 == leapMonth)
            {
                dayOfYear += leapDays;
            }
        }
        dayOfYear += day;
        doubleYears += (float)dayOfYear / daysInYear;
    }

    public WorldTime(int h, int mi, int s, float mil)
    {
        doubleYears += year;
        // doubleYears += (float)month/monthsInYear;
        doubleYears += (float)dayOfYear / daysInYear;

        hour = h;
        doubleYears += (float)hour / hoursInDay / daysInYear;
        minute = mi;
        doubleYears += (float)minute / minutesInHour / hoursInDay / daysInYear;
        second = s;
        doubleYears += (float)second / secondsInMinute / minutesInHour / hoursInDay / daysInYear;
        millisecond = mil;
        doubleYears += (float)millisecond / millisecondsInSecond / secondsInMinute / minutesInHour / hoursInDay / daysInYear;
    }
    public WorldTime(int y, int mo, int d, int h, int mi, int s, float mil)
    {
        year = y;
        doubleYears += year;
        month = mo;
        monthName = monthNames[mo - 1];
        // doubleYears += (float)month/monthsInYear;
        day = d;
        dayOfYear = 0;
        for (int i = 0; i < mo - 1; i++)
        {
            dayOfYear += daysInMonth[i];
            if (year % leapFrequency == 0 && i + 1 == leapMonth)
            {
                dayOfYear += leapDays;
            }
        }
        dayOfYear += day;
        doubleYears += (float)dayOfYear / daysInYear;

        hour = h;
        doubleYears += (float)hour / hoursInDay / daysInYear;
        minute = mi;
        doubleYears += (float)minute / minutesInHour / hoursInDay / daysInYear;
        second = s;
        doubleYears += (float)second / secondsInMinute / minutesInHour / hoursInDay / daysInYear;
        millisecond = mil;
        doubleYears += millisecond / millisecondsInSecond / secondsInMinute / minutesInHour / hoursInDay / daysInYear;
    }

    public void Add(double years)
    {
        Simplify(doubleYears + years);
    }

    public void AddDays(float days)
    {
        days /= daysInYear;
        Simplify(doubleYears + days);
    }

    public void AddHours(float hours)
    {
        hours = hours / hoursInDay / daysInYear;
        Simplify(doubleYears + hours);
    }

    void Simplify(double totalValue)
    {
        double tempTotal = totalValue;
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
                month++;
                int tempDaysInMonth = daysInMonth[i];
                if (year % leapFrequency == 0 && i + 1 == leapMonth)
                {
                    tempDaysInMonth += leapDays;
                }
                day -= tempDaysInMonth;
            }
            else break;
        }
        monthName = monthNames[month - 1];
    }
}
