using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SeaLevelServer : MonoBehaviour
{


    public static SeaLevelServer Instance {get; private set;}
    TimeServer time;
    int seaLevelAdjust;
    public InputAction seaLevelPlus, seaLevelMinus;

    [SerializeField] AnimationCurve slcLinearInt;
    [SerializeField] AnimationCurve slcSteppedInt;
    [SerializeField] AnimationCurve slcWigglyInt;
    AnimationCurve[] interpolationModes;
    string[] interpolationModeNames;
    int interpolationMode;
    public InputAction interpolationModeBtn;


    void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            time = GameObject.Find("TimeServer").GetComponent<TimeServer>();
            seaLevelAdjust = 0;
            interpolationModes  = new AnimationCurve[3] { slcLinearInt, slcSteppedInt, slcWigglyInt };
            interpolationModeNames = new string[3] { "Linear", "Stepped", "Inundation/Regression" };
            interpolationMode = 0;

        }
    }

    void OnEnable()
    {
        seaLevelPlus.Enable();
        seaLevelMinus.Enable();
        interpolationModeBtn.Enable();
    }

    void OnDisable()
    {
        seaLevelPlus.Disable();
        seaLevelMinus.Disable();
        interpolationModeBtn.Disable();
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

    public float UseInterpolation(float oldHeight, float newHeight, int pYear)
    {
        return Mathf.Lerp(oldHeight, newHeight, interpolationModes[interpolationMode].Evaluate((pYear % 1000) / 1000.0f));
    }

    public float GetGIAWaterHeight()
    {
/*        if (time.GetYear() == 20000) {
            return SLC[SLC.Length - 1] + seaLevelAdjust;
        } else {

            float timeThroughCentury = time.GetYear() % 1000;
            int SLCindex = (time.GetYear() - 5000) / 1000;
            float giaWaterHeight = Mathf.Lerp(SLC[SLCindex], SLC[SLCindex + 1], interpolationModes[interpolationMode].Evaluate(timeThroughCentury / 1000.0f));

            return giaWaterHeight + seaLevelAdjust;
        }*/
        return 0.0f + seaLevelAdjust;
    }

    public int GetSeaLevelAdjust()
    {
        return seaLevelAdjust;
    }


    void Update()
    {
        if (interpolationModeBtn.WasPressedThisFrame())
        {
            CycleInterpolationMode();
            Debug.Log("Interpolation mode is " + interpolationMode);
        }
        if (seaLevelPlus.WasPressedThisFrame())
        {
            seaLevelAdjust++;
        }
        if (seaLevelMinus.WasPressedThisFrame())
        {
            seaLevelAdjust--;
        }
    }
}
