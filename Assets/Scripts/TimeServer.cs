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
    [SerializeField]
    GameObject springImage;
    [SerializeField]
    GameObject summerImage;
    [SerializeField]
    GameObject autumnImage;
    [SerializeField]
    GameObject winterImage;
    [SerializeField]
    GameObject arrow1;
    Vector3 yearAdj = new Vector3(0.048f, 0.0f, 0.0f);





    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        year = 20000;
        day = 1;
        baseSnowline = 300;
        arrow1.transform.position = arrow1.transform.position + (yearAdj * (20000 - year));
    }

    public int GetYear() {
        return year;
    }

    public void SetYear(int pYear) {
        arrow1.transform.position = arrow1.transform.position + (yearAdj * (year - pYear));
        year = pYear;
    }

    public int GetDay() {
        return day;
    }

    public float GetSnowline() {
        return snowline;
    }

    public void IncrementYear() {
        year--;
    }

    public void IncrementDay() {
        day++;
        if (day > 365) {
            year--;
            arrow1.transform.position = arrow1.transform.position + yearAdj;
            day = 1;
        }
        season = (Seasons) (day / SEASONLENGTH);
        if (season != lastSeason) {
            switch (season)
            {
                case Seasons.Spring:
                    springImage.SetActive(true);
                    summerImage.SetActive(false);
                    autumnImage.SetActive(false);
                    winterImage.SetActive(false);
                    snowline = baseSnowline;
                    break;
                case Seasons.Summer:
                    springImage.SetActive(false);
                    summerImage.SetActive(true);
                    autumnImage.SetActive(false);
                    winterImage.SetActive(false);
                    snowline = baseSnowline * 2;
                    break;
                case Seasons.Autumn:
                    springImage.SetActive(false);
                    summerImage.SetActive(false);
                    autumnImage.SetActive(true);
                    winterImage.SetActive(false);
                    snowline = baseSnowline;
                    break;
                case Seasons.Winter:
                    springImage.SetActive(false);
                    summerImage.SetActive(false);
                    autumnImage.SetActive(false);
                    winterImage.SetActive(true);
                    snowline = baseSnowline / 2;
                    break;
                default:
                    break;
            }

        }
        lastSeason = season;
    }

    public void AdjustYear(int yearAdjust) {
        arrow1.transform.position = arrow1.transform.position + (yearAdj * (year - yearAdjust));
        year = year + yearAdjust;
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

}
