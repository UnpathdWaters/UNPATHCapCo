using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter))]
public class MenuLandscapeImport : MonoBehaviour
{
    protected FileInfo surfaceFile = null;
    protected StreamReader surfaceStream = null;
    protected string inputLine = " ";
    int widthX = 600;
    int heightZ = 600;
    float[,] depths;
    bool [,] trees;
    bool treeSet;
    bool pause;
    string[,] headerText;
    int totCols;
    int totRows;
    float noData;
    Vector3[] vertices;
    int[] triangles;
    Color[] colours;
    Color[] basecol;
    Mesh mesh;
    public Gradient gradient;
    float maxVal = -9999;
    float minVal = 9999;
    public float zScale = 0.1f;
    Vector2 clickedPoint;
    private int SEASONLENGTH = 91;
    [SerializeField]
    GameObject springImage;
    [SerializeField]
    GameObject summerImage;
    [SerializeField]
    GameObject autumnImage;
    [SerializeField]
    GameObject winterImage;
    [SerializeField]
    float coastSize;
    [SerializeField]
    float baseSnowline;
    [SerializeField]
    Camera cam;
    [SerializeField]
    GameObject arrow1;
    [SerializeField]
    GameObject glaciers20k;
    [SerializeField]
    GameObject glaciers17k;
    [SerializeField]
    GameObject glaciers15k;
    [SerializeField]
    int leftCol;
    [SerializeField]
    int rightCol;
    [SerializeField]
    int topRow;
    [SerializeField]
    int bottomRow;
    [SerializeField]
    SeaLevelServer sls;
    [SerializeField]
    int timeJumpAmt;
    [SerializeField]
    TimeServer time;

    public InputAction quitBtn;
    public InputAction loadSceneBtn;
    public InputAction timeControl;
    public InputAction timeJumpPlus;
    public InputAction timeJumpMinus;
    public float timeSpeed;

    float seaPos;
    Color seaCol = new Color(0.0f, 0.0f, 0.9f, 1.0f);
    Color coastCol = new Color(0.8f, 0.8f, 0.0f, 1.0f);
    Color autumnTrees = new Color(0.9f, 0.2f, 0.0f, 1.0f);
    Color clickCol = new Color(0.9f, 0.0f, 0.9f, 1.0f);
    Color clickNeighbour = new Color(0.9f, 0.3f, 0.9f, 1.0f);
    Color boxCol = new Color(1.0f, 0.0f, 0.0f, 1.0f);

    float snowline;
    bool quittable;
    Vector3 yearAdj = new Vector3(0.048f, 0.0f, 0.0f);
    float timeChangeAmount;



    // Start is called before the first frame update
    void Start()
    {
        snowline = baseSnowline;
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        ImportData();
        InitTimeManagement();
        CreateMesh();
        UpdateMesh();
    }

    void OnEnable()
    {
        quitBtn.Enable();
        loadSceneBtn.Enable();
        timeControl.Enable();
        timeJumpPlus.Enable();
        timeJumpMinus.Enable();
    }

    void OnDisable()
    {
        quitBtn.Disable();
        loadSceneBtn.Disable();
        timeControl.Disable();
        timeJumpPlus.Disable();
        timeJumpMinus.Disable();
    }

    void ImportData()
    {
        surfaceFile = new FileInfo ("d:/QGISdata/surface600.asc");
        surfaceStream = surfaceFile.OpenText();
        string[] hdrArray;
        depths = new float[widthX, heightZ];
        trees = new bool[widthX, heightZ];
        treeSet = false;
        headerText = new string[2,6];
        float thisval = 0.0f;
        char[] separators = new char[] { ' ', '\t', ',' };
        clickedPoint = new Vector2(0, 0);
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
        Debug.Log("Maxval is " + maxVal + " and minval is " + minVal);
    }



    void CreateMesh()
    {
        vertices = new Vector3[widthX * heightZ];
        triangles = new int[(1 + widthX * heightZ) * 6];
        colours = new Color[vertices.Length];
        basecol = new Color[vertices.Length];
        
        // Create vertices from depth array
        for (int z = 0; z < heightZ; z++)
        {
            for (int x = 0; x < widthX; x++)
            {
                vertices[x + (z * widthX)] = new Vector3(x, depths[x , z] * zScale, z);

                if (x > 0 && z > 0)
                {
                    triangles[(((x - 1) + ((z - 1) * widthX)) * 6) + 0] = x + (z * widthX); 
                    triangles[(((x - 1) + ((z - 1) * widthX)) * 6) + 1] = (x - 1) + ((z - 1) * widthX); 
                    triangles[(((x - 1) + ((z - 1) * widthX)) * 6) + 2] = (x - 1) + (z * widthX); 
                    triangles[(((x - 1) + ((z - 1) * widthX)) * 6) + 3] = x + (z * widthX); 
                    triangles[(((x - 1) + ((z - 1) * widthX)) * 6) + 4] = x + ((z - 1) * widthX); 
                    triangles[(((x - 1) + ((z - 1) * widthX)) * 6) + 5] = (x - 1) + ((z - 1) * widthX); 
                }

            }
        }
    EvaluateBaseColours();
    }

    void EvaluateBaseColours()
    {
        // Assign colours to vertices
        for (int z = 0; z < heightZ; z++)
        {
            for (int x = 0; x < widthX; x++)
            {
//                float vertHeight = Mathf.InverseLerp(minVal, maxVal, depths[x, z]);
                float vertHeight = Mathf.InverseLerp(seaPos, seaPos + snowline, depths[x, z]);
                colours[x + (z * widthX)] = AddNoiseToColor(gradient.Evaluate(vertHeight));
                basecol[x + (z * widthX)] = AddNoiseToColor(gradient.Evaluate(vertHeight));

            }
        }

    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colours;
        mesh.RecalculateNormals();
    }

    void InitTimeManagement()
    {
        seaPos = sls.GetGIAWaterHeight(time.GetYear());
        arrow1.transform.position = arrow1.transform.position + (yearAdj * (20000 - time.GetYear()));
        SetGlacierVisibility();
    }

    Color AddNoiseToColor(Color inColor)
    {
        float rRand = Random.Range(-0.03f, 0.04f);
        float gRand = Random.Range(-0.03f, 0.04f);
        float bRand = Random.Range(-0.03f, 0.04f);
        float newR = ColNormal(inColor.r + rRand);
        float newG = ColNormal(inColor.g + gRand);
        float newB = ColNormal(inColor.b + bRand);
        Color retCol = new Color(newR, newG, newB, 1);
        return retCol;        
    }

    float ColNormal(float inNum)
    {
        if (inNum < 0.0f) {
            return 0.0f;
        } else if (inNum > 1.0f) {
            return 1.0f;
        } else {
            return inNum;
        }
    }


    bool IsNeighbour(Vector2 point1, Vector2 point2) {
        if (Mathf.Abs(point1.x - point2.x) <= 1 && Mathf.Abs(point1.y - point2.y) <= 1) {
            if (point1.x == point2.x && point1.y == point2.y) {
                return false;
            } else {
                return true;
            }
        } else {
            return false;
        }
    }

    bool isBox(int x, int y)
    {
        if (x == leftCol && y >= topRow && y <= bottomRow)
        {
            return true;
        }
        if (x == rightCol && y >= topRow && y <= bottomRow)
        {
            return true;
        }
        if (y == topRow && x >= leftCol && x <= rightCol)
        {
            return true;
        }
        if (y == bottomRow && x >= leftCol && x <= rightCol)
        {
            return true;
        }
        return false;
    }

    void SetGlacierVisibility()
    {
        if (year == 20000) {
            glaciers20k.SetActive(true);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
        } else if (year > 17500) {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(true);
            glaciers15k.SetActive(false);
        } else if (year > 15000) {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(true);
        } else if (year > 12500) {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
        } else if (year > 10000) {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
        } else if (year > 7500) {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
        } else {
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
        }

    }

    void UpdateMeshColors()
    {
        float timeThroughSeason = (float) (day % SEASONLENGTH) /  (float) SEASONLENGTH;
        if (timeThroughSeason > 0.5f) {
            timeThroughSeason = 0.5f - (timeThroughSeason - 0.5f);
        }

        float timeThroughYear = (float) day / (float) 365;
        if (timeThroughYear > 0.5f) {
            timeThroughYear = 0.5f - (timeThroughYear - 0.5f);
        }

        if (day % SEASONLENGTH == 1 && season == Seasons.Autumn) {
            treeSet = false;
        } else {
            treeSet = true;
        }
//        Debug.Log("Time through season is " + timeThroughSeason);


        // Assign colours to vertices
        for (int z = 0; z < heightZ; z++)
        {
            for (int x = 0; x < widthX; x++)
            {
                if (isBox(x, z)) {
                    colours[x + (z * widthX)] = boxCol;
                } else if (x == clickedPoint.x && z == clickedPoint.y) {
                    colours[x + (z * widthX)] = clickCol;
                } else if (IsNeighbour(new Vector2(x, z), clickedPoint)) {
                    colours[x + (z * widthX)] = clickNeighbour;
                } else if (depths[x, z] < seaPos) {
                    colours[x + (z * widthX)] = seaCol;
                } else if (depths[x, z] - seaPos < coastSize) {
                    colours[x + (z * widthX)] = coastCol;
                } else {
                    switch (season)
                    {
                        case Seasons.Spring:
                            colours[x + (z * widthX)] = basecol[x + (z * widthX)];
                            break;
                        case Seasons.Summer:
                            if (depths[x, z] < (snowline + seaPos))
                            {
                                colours[x + (z * widthX)] = Color.Lerp(basecol[x + (z * widthX)], Color.yellow, (timeThroughSeason / 4.0f));
                            }
                            break;
                        case Seasons.Autumn:
                            if (!treeSet) {
                                if (depths[x, z] < (snowline + seaPos) && depths[x, z] > (seaPos + (coastSize * 5))) {
                                    if (Random.Range(0, 100) < 15) {
                                        trees[x, z] = true;
                                    } else {
                                        trees[x, z] = false;
                                    }
                                } else {
                                    trees[x, z] = false;
                                }    
                            }
                            if (trees[x, z]) {
                                colours[x + (z * widthX)] = Color.Lerp(basecol[x + (z * widthX)], autumnTrees, timeThroughSeason);
                            } else if (depths[x, z] > (seaPos + (coastSize * 5))) {
                                colours[x + (z * widthX)] = Color.Lerp(basecol[x + (z * widthX)], autumnTrees, (timeThroughSeason / 4.0f));
                            } else {
                                colours[x + (z * widthX)] = basecol[x + (z * widthX)];
                            }
                            break;
                        case Seasons.Winter:
                            float heightFac = (depths[x, z] - seaPos) / (snowline - seaPos);
                            colours[x + (z * widthX)] = Color.Lerp(basecol[x + (z * widthX)], Color.white, (timeThroughSeason + (heightFac / 2.0f)) / 1.5f);
                            break;
                        default:
                            break;
                    }
//                    if (depths[x, z] < (snowline + seaPos))
//                    {
//                        colours[x + (z * widthX)] = Color.Lerp(colours[x + (z * widthX)], Color.green, (timeThroughYear / 4.0f));
//                    }
                }
            }
        }
        mesh.colors = colours;
    }

    void Update()
    {
        bool timePeriodChanged = false;
        timeChangeAmount = timeControl.ReadValue<float>();

        if (!pause) {
            UpdateMeshColors();
            time.IncrementDay();
        }

        //Insert Raycast hit to select clicked point
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit)) {
            clickedPoint.x = hit.point.x;
            clickedPoint.y = hit.point.z;
        }

        if (clickedPoint.x < leftCol) {
            clickedPoint.x = leftCol;
        }
        if (clickedPoint.x > rightCol) {
            clickedPoint.x = rightCol;
        }
        if (clickedPoint.y < topRow) {
            clickedPoint.y = topRow;
        }
        if (clickedPoint.y > bottomRow) {
            clickedPoint.y = bottomRow;
        }

        int oldYear = year;
        year += (int) (timeChangeAmount * timeSpeed);
        if (year != oldYear) {
            timePeriodChanged = true;
            if (year > 20000) 
            {
                year = 20000;
            } else if (year < 5000) {
                year = 5000;
            }
        } else {
            timePeriodChanged = false;
        }

        
        if (quitBtn.WasReleasedThisFrame() && quittable) {
            Application.Quit();
        }
        if (loadSceneBtn.WasReleasedThisFrame()) {
            float clickX = (clickedPoint.x - leftCol) / (rightCol - leftCol);
            float clickY = (clickedPoint.y - topRow) / (bottomRow - topRow);
            Vector2 clickedPointAsPercent = new Vector2(clickX, 1.0f - clickY);
            DataStore.selectedLocation = clickedPointAsPercent;
            DataStore.cameraPosition = cam.transform.position;
            DataStore.cameraRotation = cam.transform.rotation;
            DataStore.subsequentRun = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene("LocalScene");
        }
        if (timeJumpPlus.WasReleasedThisFrame()) {
            if (year <= 20000 - timeJumpAmt) {
                year = year + timeJumpAmt;
            } else {
                year = 20000;
            }
            timePeriodChanged = true;
        }
        if (timeJumpMinus.WasReleasedThisFrame()) {
            if (year >= 5000 + timeJumpAmt) {
                year = year - timeJumpAmt;
            } else {
                year = 5000;
            }
            timePeriodChanged = true;
        }

        if (timePeriodChanged) {
            Debug.Log("Year is now " + year);
            arrow1.transform.position = arrow1.transform.position + (yearAdj * (oldYear - year));
            seaPos = sls.GetGIAWaterHeight(year);
            SetGlacierVisibility();
        }
    }
}
