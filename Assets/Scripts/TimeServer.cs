using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeServer : MonoBehaviour
{
    int year, day, hour, minute;
    Seasons season;
    int SEASONLENGTH = 91;
    GameObject arrow1;
    Vector3 yearAdj = new Vector3(0.048f, 0.0f, 0.0f);
    public static TimeServer Instance {get; private set;}
    int maxYear = 20000;
    int minYear = 5000;
    bool localMode;
    Vector3 arrowPos;
//    float[] SAT = new float[16] {1.0f, 1.0f, 1.0f, 0.916f, 0.833f, 0.833f, 0.75f, 0.666f, 0.5f, 0.5f, 0.417f, 0.0833f, 0.0833f, 0.0f, 0.0f, 0.0f};
    float[] SAT = new float[16] {1.0f, 1.0f, 1.0f, 0.916f, 0.9f, 0.833f, 0.833f, 0.0f, 0.5f, 1.0f, 0.417f, 0.0833f, 0.0833f, 0.0f, 0.0f, 0.0f};
//                                5k    6k    7k     8k     9k     10k     11k    12k   13k   14k    15k     16k       17k    18k   19k   20k    
    [SerializeField] float baseSnowline;
    [SerializeField] float seasonMultiplier;
    [SerializeField] float tempMultiplier;





    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;            
            DontDestroyOnLoad(this.gameObject);
            year = maxYear;
            day = 1;
            hour = 1;
            minute = 1;
            Debug.Log("TIME SERVER AWAKES!");
            RefreshArrowIcon();
        }
    }


    public void RefreshArrowIcon() {
        arrow1 = GameObject.Find("BlackArrow");
        arrowPos = arrow1.transform.position;
    }

    public void SetArrowPosition() {
        if (!localMode) {
            arrow1.transform.position = arrowPos + (yearAdj * (maxYear - year));
        }
    }

    public int GetYear() {
        return year;
    }

    public void SetYear(int pYear) {
        year = ValidYear(pYear);
        SetArrowPosition();
    }

    public int GetDay() {
        return day;
    }

    public int GetHour() {
        return hour;
    }

    public int GetMinute() {
        return minute;
    }

    public void SetLocalMode(bool pLocal) {
        localMode = pLocal;
    }

    public float GetSnowline() {
//        Debug.Log("base " + baseSnowline + ",temp " + GetTempFactor() + ",snow " + GetSnowFactor());
        return baseSnowline + ((GetTempFactor() * tempMultiplier) - (tempMultiplier / 2)) + ((GetSnowFactor() * seasonMultiplier) - (seasonMultiplier / 2));
    }

    public float GetMaxSnowline() {
        return baseSnowline + ((GetTempFactor() * tempMultiplier) - (tempMultiplier / 2)) + ((1.0f * seasonMultiplier) - (seasonMultiplier / 2));
    }

    public float GetMinSnowline() {
        return baseSnowline + ((GetTempFactor() * tempMultiplier) - (tempMultiplier / 2)) + (0.0f - (seasonMultiplier / 2));
    }

    public float GetMidSnowline() {
        return baseSnowline;
    }

    public void IncrementYear() {
        year = ValidYear(--year);
        Debug.Log("Year is " + year);
    }

    public void IncrementDay() {
        day++;
        if (day > 365) {
            if (localMode) {
                year = ValidYear(--year);
            }
            SetArrowPosition();
            day = 1;
        }
        season = (Seasons) (day / SEASONLENGTH);
//        Debug.Log("Day is " + day + " and snowfactor is " + GetSnowFactor() + " so snowline is " + GetSnowline());
    }

    public void IncrementMinute() {
        minute++;
        if (minute > 60) {
            IncrementHour();
            minute = 1;
        }
    }

    public void IncrementHour() {
        hour++;
        if (hour > 24) {
            IncrementDay();
            hour = 1;
        }
    }


    public void AdjustYear(int yearAdjust) {
        year = ValidYear(year + yearAdjust);
        SetArrowPosition();
    }

    public Seasons GetSeason() {
        return season;
    }

    public bool IsSpring() {
        if (season == Seasons.Spring) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsSummer() {
        if (season == Seasons.Summer) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsAutumn() {
        if (season == Seasons.Autumn) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsWinter() {
        if (season == Seasons.Winter) {
            return true;
        } else {
            return false;
        }
    }

    public bool FirstDayOfSeason(int updateFreq) {
        if (day % SEASONLENGTH <= updateFreq) {
            return true;
        } else {
            return false;
        }
    }

    public float GetSnowFactor() {
        if (day < 183) {
            return 1.0f - ((183.0f - (float)day) / 183.0f);
        } else {
            return 1.0f - (((float)day - 183.0f) / 183.0f);
        }
    }

    public float GetTimeThroughSeason() {
        float timeThroughSeason = (float) (day % SEASONLENGTH) /  (float) SEASONLENGTH;
        if (timeThroughSeason > 0.5f) {
            timeThroughSeason = 0.5f - (timeThroughSeason - 0.5f);
        }
        return timeThroughSeason;
    }

    public float GetTimeThroughYear() {
        float timeThroughYear = (float) day / (float) 365;
        if (timeThroughYear > 0.5f) {
            timeThroughYear = 0.5f - (timeThroughYear - 0.5f);
        }
        return timeThroughYear;
    }

    public int ValidYear(int pYear) {
        if (pYear > maxYear) {
            return maxYear;
        } else if (pYear < minYear) {
            return minYear;
        } else {
            return pYear;
        }
    }

    public float GetTempFactor() {
        int SATindex = (year - 5000) / 1000;
        float SATfudge = 0.0f;
//        float SATfudge = (float)(year % 1000) / 1000.0f;
//        Debug.Log("Year is " + year + " so satfudge is " + SATfudge);
        float SATfactor;
        if (SATindex > 0)
        {
            SATfactor = Mathf.Lerp(SAT[SATindex], SAT[SATindex - 1], SATfudge);
        } else {
            SATfactor = SAT[0];
        }
//        Debug.Log(SAT[SATindex] + " " + SAT[SATindex - 1] + " " + SATfudge);
        return SATfactor;
    }
}

public enum Seasons { Spring, Summer, Autumn, Winter }

