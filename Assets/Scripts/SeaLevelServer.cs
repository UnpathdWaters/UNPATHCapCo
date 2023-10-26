using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaLevelServer : MonoBehaviour
{
    static readonly float[] SLC = new float[21]{ 0.6f, 0.8f, 1.1f, 1.5f, 1.7f, 2.4f, 3.3f, 7.8f, 8.8f, 8.1f, 13.3f, 5.6f, 4.7f, 14.4f, 14.2f, 3.2f, 4.0f, 3.6f, 1.2f, 0.6f, 0.0f };

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public float GetGIAWaterHeight(int year)
    {
        float giaWaterHeight = 0;
        for (int x = 0; x < (year / 1000); x++)
        {
            giaWaterHeight = giaWaterHeight - (SLC[x]);
        }
        giaWaterHeight = giaWaterHeight - ((year % 1000) * (SLC[year / 1000] / 1000));
        return giaWaterHeight;
    }
}
