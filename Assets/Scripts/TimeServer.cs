using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeServer : MonoBehaviour
{
    int year, day;
    enum Seasons { Winter, Spring, Summer, Autumn };
    Seasons season, lastSeason;


    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        year = 20000;
        day = 1;
    }

    public int GetYear() {
        return year;
    }

    public void SetYear(int pYear) {
        year = pYear;
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
                    EvaluateBaseColours();
                    break;
                case Seasons.Summer:
                    springImage.SetActive(false);
                    summerImage.SetActive(true);
                    autumnImage.SetActive(false);
                    winterImage.SetActive(false);
                    snowline = baseSnowline * 2;
                    EvaluateBaseColours();
                    break;
                case Seasons.Autumn:
                    springImage.SetActive(false);
                    summerImage.SetActive(false);
                    autumnImage.SetActive(true);
                    winterImage.SetActive(false);
                    snowline = baseSnowline;
                    EvaluateBaseColours();
                    break;
                case Seasons.Winter:
                    springImage.SetActive(false);
                    summerImage.SetActive(false);
                    autumnImage.SetActive(false);
                    winterImage.SetActive(true);
                    snowline = baseSnowline / 2;
                    EvaluateBaseColours();
                    break;
                default:
                    EvaluateBaseColours();
                    break;
            }

        }
        lastSeason = season;
    }

    public void AdjustYear(int yearAdjust) {
        year = year + yearAdjust;
    }

    public Seasons GetSeason() {
        return season;
    }

}
