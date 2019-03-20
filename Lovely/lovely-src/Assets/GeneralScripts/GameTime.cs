using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTime : _MasterComponent<GameTime>
{
    //add info about seasons, eclipses, astrological events

    public static Cycle CycleName { get { return (Cycle)Mathf.FloorToInt((float)Cycle); } }
    public static Week WeekName { get { return (Week)Mathf.FloorToInt(Week); } }

    public static double Cycle { get { return (elapsedGameTime * SecondsToMinuites * MinuitesToHours * HoursToDays * DaysToWeeks * WeeksToMonths * MonthsToYears) % CyclesToYears; } }
    //cycle should be based on the moon cycle opposing the solar cycle

    //solar time
    public static float Year { get { return elapsedGameTime * SecondsToMinuites * MinuitesToHours * HoursToDays * DaysToWeeks * WeeksToMonths * MonthsToYears; } }
    public static float Month { get { return (elapsedGameTime * SecondsToMinuites * MinuitesToHours * HoursToDays * DaysToWeeks * WeeksToMonths) % YearsToMonths; } }
    public static float Week { get { return (elapsedGameTime * SecondsToMinuites * MinuitesToHours * HoursToDays * DaysToWeeks) % MonthsToWeeks; } }
    public static float Day { get { return (elapsedGameTime * SecondsToMinuites * MinuitesToHours * HoursToDays) % MonthsToDays; } }
    public static float Hour { get { return (elapsedGameTime * SecondsToMinuites * MinuitesToHours) % DaysToHours; } }
    public static float Minute { get { return (elapsedGameTime * SecondsToMinuites) % HoursToMinuites; } }
    public static float Second { get { return elapsedGameTime % MinuitesToSeconds; } }

    public static Season Season { get { return (Season)Mathf.FloorToInt((Year % 1) * 4); } }

    static float _elapsedTimeRealTime = 0;

    /// <summary>
    /// in seconds
    /// </summary>
    public static float elapsedRealTime { get { return _elapsedTimeRealTime; } }
    /// <summary>
    /// in seconds
    /// </summary>
    public static float elapsedGameTime { get { return _elapsedTimeRealTime * RealTimeToGameTimeRate; } }

    public static float timeScale
    {
        get
        {
            return Time.timeScale;
        }

        set
        {
            var newTimeScale = Mathf.Clamp(value, 0f, 100f);
            Time.timeScale = newTimeScale;
            Time.fixedDeltaTime = newTimeScale;
        }
    }



    public const float RealTimeToGameTimeRate = 600f / 1f;
    public const float GameTimeToRealTimeRate = 1 / RealTimeToGameTimeRate;

    public static readonly float MinuitesToSeconds = 60;
    public static readonly float HoursToMinuites = 60;
    public static readonly float DaysToHours = 12;//period of solar pos function
    public static readonly float WeeksToDays = 6;
    public static readonly float MonthsToWeeks = System.Enum.GetValues(typeof(Week)).Length;
    public static readonly float YearsToMonths = 4;
    public static readonly float CyclesToYears = System.Enum.GetValues(typeof(Cycle)).Length;

    public static readonly float MonthsToDays = MonthsToWeeks * WeeksToDays;
    public static readonly float YearsToDays = YearsToMonths * MonthsToDays;
    public static readonly float CyclesToDays = CyclesToYears * YearsToDays;

    public static readonly float SecondsToMinuites = 1 / MinuitesToSeconds;
    public static readonly float MinuitesToHours = 1 / HoursToMinuites;
    public static readonly float HoursToDays = 1 / DaysToHours;
    public static readonly float DaysToWeeks = 1 / WeeksToDays;
    public static readonly float WeeksToMonths = 1 / MonthsToWeeks;
    public static readonly float MonthsToYears = 1 / YearsToMonths;
    public static readonly float YearsToCycles = 1 / CyclesToYears;


    static readonly float[] conversions = new float[]
    { SecondsToMinuites, MinuitesToHours, HoursToDays, DaysToWeeks, WeeksToMonths, MonthsToYears, YearsToCycles };

    //one day thats not actually a day, where a cataclysm happens each cycle

    public static float Convert(float length, TimeUnit sourceUnits, TimeUnit resultUnits)
    {
        //8 is he number of units
        var realTimeToGameTime = (int)sourceUnits >= 8 && (int)resultUnits < 8;
        var gameTimeToRealTime = (int)sourceUnits < 8 && (int)resultUnits >= 8;

        //if distance is positive, we are going from small units to big units, thus multiply by conversion rate
        //if distance is negative, we are going from big units to small units, thus divide by conversion rate
        var distance = ((int)resultUnits % 8) - ((int)sourceUnits % 8);

        var conversionRate = 1f;
        for (int i = 0; i < Mathf.Abs(distance); i++)
        {
            conversionRate *= conversions[i];
        }

        if (distance < 0) conversionRate = 1f / conversionRate;

        if (realTimeToGameTime) conversionRate *= RealTimeToGameTimeRate;
        if (gameTimeToRealTime) conversionRate *= GameTimeToRealTimeRate;

        return length * conversionRate;
    }

    public static bool IsBetweenHours(float queriedTime, float startTime, float endTime)
    {
        queriedTime = Mathf.Clamp(queriedTime, 0f, DaysToHours);
        startTime = Mathf.Clamp(startTime, 0f, DaysToHours);
        endTime = Mathf.Clamp(endTime, 0f, DaysToHours);

        var result = false;
        if (startTime > endTime)
        {
            if (queriedTime > startTime || queriedTime < endTime) result = true;
        }
        if (startTime < endTime)
        {
            if (queriedTime > startTime && queriedTime < endTime) result = true;
        }
        return result;
    }

    public GameTime() { }


    [SerializeField]
    float timeScaleInEditor = 1f;

    void Update()
    {
        if (timeScale != timeScaleInEditor)
            timeScale = timeScaleInEditor;
        gameObject.DisplayTextComponent(this);
        _elapsedTimeRealTime += Time.deltaTime;
    }

    public static float DeltaTimeGameSeconds { get { return Time.deltaTime * RealTimeToGameTimeRate; } }
    public static float DeltaTimeGameMinuites { get { return DeltaTimeGameSeconds * SecondsToMinuites; } }
    public static float DeltaTimeGameHours { get { return DeltaTimeGameSeconds * SecondsToMinuites * MinuitesToHours; } }
    public static float DeltaTimeGameDays { get { return DeltaTimeGameHours * HoursToDays; } }
    public static float DeltaTimeGameWeeks { get { return DeltaTimeGameDays * DaysToWeeks; } }
    public static float DeltaTimeGameMonths { get { return DeltaTimeGameWeeks * WeeksToMonths; } }
    public static float DeltaTimeGameYears { get { return DeltaTimeGameMonths * MonthsToYears; } }
    public static float DeltaTimeGameCycles { get { return DeltaTimeGameYears * YearsToCycles; } }

    public static float DeltaTimeRealSeconds { get { return Time.deltaTime; } }
    public static float DeltaTimeRealMinuites { get { return DeltaTimeRealSeconds * 1 / 60; } }
    public static float DeltaTimeRealHours { get { return DeltaTimeRealMinuites * 1 / 60; } }
    public static float DeltaTimeRealDays { get { return DeltaTimeRealHours * 1 / 24; } }
    public static float DeltaTimeRealWeeks { get { return DeltaTimeRealDays * 1 / 7; } }
    public static float DeltaTimeRealMonths { get { return DeltaTimeRealWeeks * 1 / 4; } }
    public static float DeltaTimeRealYears { get { return DeltaTimeRealMonths * 1 / 12; } }



    public override string ToString()
    {
        string s =
            "CycleName: " + CycleName + "\n" +
            "WeekName: " + WeekName + "\n" +
            "Cycle: " + Cycle + "\n" +
            "Year: " + Year + "\n" +
            "Week: " + Week + "\n" +
            "Day: " + Day + "\n" +
            "Hour: " + Hour + "\n" +
            "Minute: " + Minute + "\n" +
            "Second: " + Second + "\n" +
            "elapsedRealTime: " + elapsedRealTime + "\n" +
            "elapsedGameTime: " + elapsedGameTime + "\n" +
            "timeScale: " + timeScale
          ;
        return s;
    }

}


public enum TimeUnit
{
    SecondGameTime = 0, MinuteGameTime, HourGameTime, DayGameTime, WeekGameTime, MonthGameTime, YearGameTime, CycleGameTime,

    SecondRealTime, MinuteRealTime, HourRealTime, DayRealTime, WeekRealTime, MonthRealTime, YearRealTime, CycleRealTime
}


public enum Cycle
{
    Air = 0, Water, Earth, Fire, Chi
}


public enum Week
{
    Phoenix = 0, Boar, Stag, Tiger, Horse, Monkey
}

public enum Season
{
    Spring = 0, Summer, Fall, Winter
}