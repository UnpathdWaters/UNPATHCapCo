using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaLevelServer : MonoBehaviour
{
    public static SeaLevelServer Instance {get; private set;}
//    static readonly float[] SLC = new float[21]{ 0.6f, 0.8f, 1.1f, 1.5f, 1.7f, 2.4f, 3.3f, 7.8f, 8.8f, 8.1f, 13.3f, 5.6f, 4.7f, 14.4f, 14.2f, 3.2f, 4.0f, 3.6f, 1.2f, 0.6f, 0.0f };
    static readonly float[] SLC = new float[16] { -6.97f, -9.89f, -13.80f, -22.24f, -31.82f, -41.27f, -53.46f, -55.82f, -57.85f, -63.01f, -76.72f, -90.09f, -92.60f, -94.89f, -93.14f, -90.20f };
    TimeServer time;

    void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            time = GameObject.Find("TimeServer").GetComponent<TimeServer>();
        }
    }

    public float GetGIAWaterHeight()
    {
        if (time.GetYear() == 20000) {
            return SLC[SLC.Length - 1];
        } else {
            int SLCindex = (time.GetYear() - 5000) / 1000;
            float giaWaterHeight = SLC[SLCindex];
            giaWaterHeight = giaWaterHeight - (((SLC[SLCindex] - SLC[SLCindex + 1]) / 1000) * (time.GetYear() % 1000));
            return giaWaterHeight;
        }
    }
}
