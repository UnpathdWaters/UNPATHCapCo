using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoPanel : MonoBehaviour
{

    [SerializeField] GameObject springImage;
    [SerializeField] GameObject summerImage;
    [SerializeField] GameObject autumnImage;
    [SerializeField] GameObject winterImage;
    [SerializeField] TMP_Text interpolationModeTMP;
    [SerializeField] TMP_Text seaLevelAdjustTMP;
    [SerializeField] TMP_Text fpsTMP;
    TimeServer time;
    SeaLevelServer sls;

    Seasons season, lastSeason;
    string interpolationModeText;


    // Start is called before the first frame update
    void Start()
    {
        time = GameObject.Find("TimeServer").GetComponent<TimeServer>();
        sls = GameObject.Find("SeaLevelServer").GetComponent<SeaLevelServer>();
        season = time.GetSeason();
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


    // Update is called once per frame
    void Update()
    {
        season = time.GetSeason();
        if (season != lastSeason)
        {
            SetSeasonIcons();
        }
        interpolationModeText = sls.GetInterpolationModeName();
        interpolationModeTMP.text = "Interpolation mode: " + interpolationModeText;
        seaLevelAdjustTMP.text = "Sea Level Adjust: " + sls.GetSeaLevelAdjust() + "m";
        fpsTMP.text = "FPS: " + (int)(1f / Time.unscaledDeltaTime);
    }
}
