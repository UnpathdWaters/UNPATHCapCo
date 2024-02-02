using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.InputSystem;

public class MenuLandscapeImport : MonoBehaviour
{
    protected FileInfo surfaceFile = null;
    protected StreamReader surfaceStream = null;
    protected string inputLine = " ";
    int widthX = 600;
    int heightZ = 600;
    float[,] depths;
    bool pause;
    string[,] headerText;
    int totCols;
    int totRows;
    float noData;
    float maxVal = -9999;
    float minVal = 9999;
    public float zScale = 0.1f;
    [SerializeField] GameObject terrainCube;
    [SerializeField] Camera cam;
    [SerializeField] GameObject glaciers20k;
    [SerializeField] GameObject glaciers17k;
    [SerializeField] GameObject glaciers15k;
    [SerializeField] int leftCol;
    [SerializeField] int rightCol;
    [SerializeField] int topRow;
    [SerializeField] int bottomRow;
    [SerializeField] int timeJumpAmt;
    [SerializeField] GameObject terrainControllerGO;
    TimeServer time;
    SeaLevelServer sls;
    TerrainController terrainController;

    public InputAction quitBtn;
    public InputAction loadSceneBtn;
    public InputAction timeControl;
    public InputAction timeJumpPlus;
    public InputAction timeJumpMinus;
    public InputAction controlsBtn;
    public float timeSpeed;
    public GameObject loadingScreen;
    public GameObject controlScreen;
    public float gradientAnchor;
    bool quittable;
    int timeChangeAmount;



    // Start is called before the first frame update
    void Start()
    {
        sls = GameObject.Find("SeaLevelServer").GetComponent<SeaLevelServer>();
        time = GameObject.Find("TimeServer").GetComponent<TimeServer>();
        terrainController = terrainControllerGO.GetComponent<TerrainController>();
        ImportData();
        CreateTerrainCubes();
        InitTimeManagement();
        quittable = false;
    }

    void OnEnable()
    {
        quitBtn.Enable();
        loadSceneBtn.Enable();
        timeControl.Enable();
        timeJumpPlus.Enable();
        timeJumpMinus.Enable();
        controlsBtn.Enable();
    }

    void OnDisable()
    {
        quitBtn.Disable();
        loadSceneBtn.Disable();
        timeControl.Disable();
        timeJumpPlus.Disable();
        timeJumpMinus.Disable();
        controlsBtn.Disable();
    }

    void ImportData()
    {
        surfaceFile = new FileInfo (".\\surface600.asc");
        surfaceStream = surfaceFile.OpenText();
        string[] hdrArray;
        depths = new float[widthX, heightZ];
        headerText = new string[2,6];
        float thisval = 0.0f;
        char[] separators = new char[] { ' ', '\t', ',' };
        if (DataStore.subsequentRun)
        {
            cam.transform.position = DataStore.cameraPosition;
            cam.transform.rotation = DataStore.cameraRotation;
        }

        //Read ESRI ASCII header
        for (int headline = 0; headline < 6; headline++)
        {
            inputLine = surfaceStream.ReadLine();
            hdrArray = inputLine.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
            headerText[0,headline] = hdrArray[0];
            headerText[1,headline] = hdrArray[1];
        }


        totCols = int.Parse(headerText[1,0]);
        totRows = int.Parse(headerText[1,1]);
        noData = float.Parse(headerText[1,5]);

        Debug.Log("Input file Cols = " + totCols);
        Debug.Log("Input file Rows = " + totRows);
        Debug.Log("Input noData val is " + noData);

        string[] readArray = new string[totCols];

        int xCount = 0;
        int zCount = heightZ - 1;
        for (int z = 0; z < totRows; z++)
        {
            inputLine = surfaceStream.ReadLine();
            xCount = 0;
            readArray = inputLine.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
            for (int x = 0; x < totCols; x++)
            {
                thisval = float.Parse(readArray[x]);
                depths[xCount, zCount] = thisval;
                if (thisval > maxVal)
                {
                    maxVal = thisval;
                }
                if (thisval < minVal)
                {
                    minVal = thisval;
                }
                xCount++;
            }
            zCount--;
        }
        terrainController.SetMaxHeight(maxVal);
        terrainController.SetMinHeight(minVal);

        Debug.Log("Maxval is " + maxVal + " and minval is " + minVal);
    }

    void CreateTerrainCubes()
    {
        for (int z = 0; z < totRows; z++)
        {
            for (int x = 0; x < totCols; x++)
            {
                Vector3 cubeLoc = new Vector3(x, depths[x, z] * zScale, z);
                Instantiate(terrainCube, cubeLoc, Quaternion.identity);
            }
        }
    }

    void InitTimeManagement()
    {
        Debug.Log("Year at Init is " + time.GetYear());
        time.SetLocalMode(false);
        SetGlacierVisibility();
        time.RefreshArrowIcon();
        time.SetArrowPosition();
    }


    void SetGlacierVisibility()
    {
        if (time.GetYear() == 20000) {
            glaciers20k.SetActive(true);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
        } else if (time.GetYear() > 17500) {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(true);
            glaciers15k.SetActive(false);
        } else if (time.GetYear() > 15000) {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(true);
        } else if (time.GetYear() > 12500) {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
        } else if (time.GetYear() > 10000) {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
        } else if (time.GetYear() > 7500) {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
        } else {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
        }
    }


    void Update()
    {
        bool timePeriodChanged = false;
        timeChangeAmount = (int) (timeControl.ReadValue<float>() * timeSpeed);

        if (timeChangeAmount != 0) {
            time.AdjustYear(timeChangeAmount);
            timePeriodChanged = true;
        }

        if (!pause) {
            time.IncrementDay();
        }

        if (quitBtn.WasReleasedThisFrame() && quittable) {
            UnityEngine.SceneManagement.SceneManager.LoadScene("01IntroScene");
        }
        if (loadSceneBtn.WasReleasedThisFrame()) {
            UnityEngine.SceneManagement.SceneManager.LoadScene("03LocalScene");
        }

        if (timeJumpPlus.WasReleasedThisFrame()) {
            time.AdjustYear(timeJumpAmt);
            timePeriodChanged = true;
        }
        if (timeJumpMinus.WasReleasedThisFrame()) {
            time.AdjustYear(0 - timeJumpAmt);
            timePeriodChanged = true;
        }
        if (controlsBtn.IsPressed()) {
            controlScreen.SetActive(true);
        } else {
            controlScreen.SetActive(false);
        }

        if (timePeriodChanged) {
            Debug.Log("Year is now " + time.GetYear());
            SetGlacierVisibility();
        }
        quittable = true;
    }
}
