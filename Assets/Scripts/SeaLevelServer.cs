using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SeaLevelServer : MonoBehaviour
{

    public InputAction interpolationModeBtn;

    public static SeaLevelServer Instance {get; private set;}
//    static readonly float[] SLC = new float[21]{ 0.6f, 0.8f, 1.1f, 1.5f, 1.7f, 2.4f, 3.3f, 7.8f, 8.8f, 8.1f, 13.3f, 5.6f, 4.7f, 14.4f, 14.2f, 3.2f, 4.0f, 3.6f, 1.2f, 0.6f, 0.0f };
    float[] SLC = new float[16] { -6.97f, -9.89f, -13.80f, -22.24f, -31.82f, -41.27f, -53.46f, -55.82f, -57.85f, -63.01f, -76.72f, -90.09f, -92.60f, -94.89f, -93.14f, -90.20f };
    TimeServer time;
    [SerializeField] AnimationCurve slcLinearInt;
    [SerializeField] AnimationCurve slcSteppedInt;
    [SerializeField] AnimationCurve slcWigglyInt;
    AnimationCurve[] interpolationModes;
    string[] interpolationModeNames;
    int interpolationMode;

    void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            time = GameObject.Find("TimeServer").GetComponent<TimeServer>();
            interpolationModes  = new AnimationCurve[3] { slcLinearInt, slcSteppedInt, slcWigglyInt };
            interpolationModeNames = new string[3] { "Linear", "Stepped", "Inundation/Regression" };
            interpolationMode = 0;
        }
    }

    void OnEnable()
    {
        interpolationModeBtn.Enable();
    }

    void OnDisable()
    {
        interpolationModeBtn.Disable();
    }

    public float GetGIAWaterHeight()
    {
        if (time.GetYear() == 20000) {
            return SLC[SLC.Length - 1];
        } else {

            float timeThroughCentury = time.GetYear() % 1000;
            int SLCindex = (time.GetYear() - 5000) / 1000;
            float giaWaterHeight = Mathf.Lerp(SLC[SLCindex], SLC[SLCindex + 1], interpolationModes[interpolationMode].Evaluate(timeThroughCentury / 1000.0f));



/*            int SLCindex = (time.GetYear() - 5000) / 1000;
            float giaWaterHeight = SLC[SLCindex];
            giaWaterHeight = giaWaterHeight - (((SLC[SLCindex] - SLC[SLCindex + 1]) / 1000) * (time.GetYear() % 1000));*/




            return giaWaterHeight;
        }
    }

    public void SetSLC(float[] newSLC)
    {
        SLC = newSLC;
        for (int x = 0; x < newSLC.Length; x++)
        {
            Debug.Log(SLC[x]);
        }
    }

    public float[] GetSLC()
    {
        return SLC;
    }

    public string GetInterpolationModeName()
    {
        return interpolationModeNames[interpolationMode];
    }

    void CycleInterpolationMode()
    {
        interpolationMode++;
        if (interpolationMode == interpolationModes.Length)
        {
            interpolationMode = 0;
        }
    }

    void Update()
    {
        if (interpolationModeBtn.WasPressedThisFrame())
        {
            CycleInterpolationMode();
            Debug.Log("Interpolation mode is " + interpolationMode);
        }
    }
}
