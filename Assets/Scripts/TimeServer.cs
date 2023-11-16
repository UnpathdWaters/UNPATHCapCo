using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeServer : MonoBehaviour
{
    int year, day;
    public enum Seasons { Winter, Spring, Summer, Autumn };
    Seasons season, lastSeason;
    int SEASONLENGTH = 91;
    float snowline, baseSnowline;
    GameObject springImage;
    GameObject summerImage;
    GameObject autumnImage;
    GameObject winterImage;
    GameObject arrow1;
    Vector3 yearAdj = new Vector3(0.048f, 0.0f, 0.0f);
    public static TimeServer Instance {get; private set;}
    int maxYear = 20000;
    int minYear = 5000;
    bool localMode;
    Vector3 arrowPos;





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
            baseSnowline = 300;
            Debug.Log("TIME SERVER AWAKES!");
            RefreshSeasonIcons();
            RefreshArrowIcon();
        }
    }

    public void RefreshSeasonIcons() {
        springImage = GameObject.Find("SpringImage");
        summerImage = GameObject.Find("SummerImage");
        autumnImage = GameObject.Find("AutumnImage");
        winterImage = GameObject.Find("WinterImage");
    }

    public void RefreshArrowIcon() {
        arrow1 = GameObject.Find("BlackArrow");
        arrowPos = arrow1.transform.position;
//        arrow1.transform.position = arrow1.transform.position + (yearAdj * (20000 - year));
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

    public void SetLocalMode(bool pLocal) {
        localMode = pLocal;
    }

    public float GetSnowline() {
        switch (season)
        {
            case Seasons.Spring:
                return baseSnowline;
            case Seasons.Summer:
                return baseSnowline * 2;
            case Seasons.Autumn:
                return baseSnowline;
            case Seasons.Winter:
                return baseSnowline / 2;
            default:
                return baseSnowline;
        }
    }

    public void IncrementYear() {
        year = ValidYear(year--);
    }

    public void IncrementDay() {
        day++;
        if (day > 365) {
            year = ValidYear(year--);
            SetArrowPosition();
            day = 1;
        }
        season = (Seasons) (day / SEASONLENGTH);
        if (season != lastSeason) {
            SetSeasonIcons();
        }
        lastSeason = season;
    }

    public void SetSeasonIcons() {
        switch (season)
        {
            case Seasons.Spring:
                springImage.SetActive(true);
                summerImage.SetActive(false);
                autumnImage.SetActive(false);
                winterImage.SetActive(false);
                break;
            case Seasons.Summer:
                springImage.SetActive(false);
                summerImage.SetActive(true);
                autumnImage.SetActive(false);
                winterImage.SetActive(false);
                break;
            case Seasons.Autumn:
                springImage.SetActive(false);
                summerImage.SetActive(false);
                autumnImage.SetActive(true);
                winterImage.SetActive(false);
                break;
            case Seasons.Winter:
                springImage.SetActive(false);
                summerImage.SetActive(false);
                autumnImage.SetActive(false);
                winterImage.SetActive(true);
                break;
            default:
                break;
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

    public bool FirstDayOfSeason() {
        if (day % SEASONLENGTH == 1) {
            return true;
        } else {
            return false;
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

}
