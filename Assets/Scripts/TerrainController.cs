using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    float maxHeight, minHeight;
    float seaLevel;
    float yScale;
    TimeServer time;
    SeaLevelServer sls;


    void Start()
    {
        sls = GameObject.Find("SeaLevelServer").GetComponent<SeaLevelServer>();
        time = GameObject.Find("TimeServer").GetComponent<TimeServer>();
    }

    public void SetYScale(float pYS)
    {
        yScale = pYS;
    }

    public void SetMaxHeight(float pMH)
    {
        maxHeight = pMH;
    }

    public void SetMinHeight(float pMH)
    {
        minHeight = pMH;
    }

    public float GetMaxHeight()
    {
        return maxHeight;
    }

    public float GetMinHeight()
    {
        return minHeight;
    }

    public float GetHeightRange()
    {
        return maxHeight - minHeight;
    }

    public float GetSeaLevelPos()
    {
        return seaLevel * yScale;
    }

    void Update()
    {
        seaLevel = sls.GetGIAWaterHeight();
    }
}
