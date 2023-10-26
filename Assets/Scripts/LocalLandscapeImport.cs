using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter))]
public class LocalLandscapeImport : MonoBehaviour
{
    protected FileInfo surfaceFile = null;
    protected StreamReader surfaceStream = null;
    protected string inputLine = " ";
    int widthX = 512;
    int heightZ = 512;
    float[,] depths;
    bool pause;
    string[,] headerText;
    int totCols;
    int totRows;
    float noData;
    Vector3[] vertices;
    int[] triangles;
    Color[] colours;
    Mesh mesh;
    float maxVal = -9999;
    float minVal = 9999;
    float midVal = 0;
    public float zScale = 0.1f;
    public Gradient gradient;
    int year, day;
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
    GameObject sea;
    [SerializeField]
    float coastSize;
    [SerializeField]
    float baseSnowline;
    [SerializeField]
    Camera cam;

    float seaPos;
    public Color seaCol;
    public Color coastCol;
    public Color marshCol;

    enum Seasons { Winter, Spring, Summer, Autumn };
    Seasons season, lastSeason;
    float snowline;

    public Texture2D landuseMap;
    bool[,] river;
    bool[,] marsh;

    public GameObject tree;
    public GameObject reeds;

    public int reedDensity;
    public int treeDensity;

    public InputAction quitBtn;

    SeaLevelServer sls;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Location is " + DataStore.selectedLocation);
        Debug.Log("Year is " + DataStore.selectedYear);

        snowline = baseSnowline;
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        river = new bool[widthX, heightZ];
        marsh = new bool[widthX, heightZ];
        sls = GameObject.Find("SeaLevelServer").GetComponent<SeaLevelServer>();

        ImportLocalSection();
        InitTimeManagement();
        CreateMesh();
        UpdateMesh();
        CreateTrees();
        CreateReeds();
        UpdateMeshColors();
        Debug.Log("Maxval is " + maxVal + " and minval is " + minVal + " and midVal is " + midVal);
    }

    void OnEnable()
    {
        quitBtn.Enable();
    }

    void OnDisable()
    {
        quitBtn.Disable();
    }

    void ImportLocalSection()
    {
        surfaceFile = new FileInfo ("D:/QGISdata/SquareSelectableArea18764.asc");
        surfaceStream = surfaceFile.OpenText();
        string[] hdrArray;
        depths = new float[widthX, heightZ];
        headerText = new string[2,6];
        float thisval = 0.0f;
        char[] separators = new char[] { ' ', '\t', ',' };

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

        int selectedCol = (int) (DataStore.selectedLocation.x * totCols);
        int selectedRow = (int) (DataStore.selectedLocation.y * totRows);
        int startX = selectedCol - (widthX / 2);
        int endX = selectedCol + (widthX / 2);
        int startY = selectedRow - (heightZ / 2);
        int endY = selectedRow + (heightZ / 2);

        Debug.Log("selectedCol is " + selectedCol + "and Row is " + selectedRow + "so X goes from " + startX + "-" + endX + "and Y goes from" + startY + "-" + endY);

        if (startX < 0)
        {
            startX = 0;
            endX = widthX - 1;
        } else if (endX > totCols - 1)
        {
            startX = totCols - 1 - widthX;
            endX = totCols -1;
        }

        if (startY < 0)
        {
            startY = 0;
            endY = heightZ - 1;
        } else if (endY > totRows - 1)
        {
            startY = totRows - 1 - heightZ;
            endY = totRows - 1;
        }


        string[] readArray = new string[totCols];
        float[] tempArray = new float[widthX * heightZ];

        int xCount = 0;
        int zCount = heightZ - 1;
        for (int z = 0; z < totRows; z++)
        {
            inputLine = surfaceStream.ReadLine();
            if (z > startY && z < endY)
            {
                xCount = 0;
                readArray = inputLine.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
                for (int x = startX; x < endX; x++)
                {
                    thisval = float.Parse(readArray[x]);
                    depths[xCount, zCount] = AddNoiseToDepths(thisval);
                    tempArray[xCount + (zCount * widthX)] = thisval;
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
        }
        midVal = CalculateMedian(tempArray);
    }

    float AddNoiseToDepths(float depth)
    {
        return depth + UnityEngine.Random.Range(-0.5f, 0.5f);
    }

    void CreateMesh()
    {
        vertices = new Vector3[widthX * heightZ];
        triangles = new int[(1 + widthX * heightZ) * 6];
        colours = new Color[vertices.Length];
        
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
        year = DataStore.selectedYear;
        day = 1;
        season = Seasons.Winter;
        lastSeason = season;
        seaPos = sls.GetGIAWaterHeight(year);
    }

    Color AddNoiseToColor(Color inColor)
    {
        float rRand = UnityEngine.Random.Range(-0.03f, 0.04f);
        float gRand = UnityEngine.Random.Range(-0.03f, 0.04f);
        float bRand = UnityEngine.Random.Range(-0.03f, 0.04f);
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

    void IncrementTime()
    {
        day++;
        if (day > 365) {
            year--;
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

    Vector3 JigglePosition(Vector3 inVec)
    {
        Vector3 adj = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
        return inVec + adj;
    }

    void CreateTrees()
    {
        for (int y = 2; y < landuseMap.height - 2; y++){
            for (int x = 2; x < landuseMap.width - 2; x++) {
                if (depths[x, y] > midVal && !river[x, y] && !marsh[x, y]) {
                    if (UnityEngine.Random.Range(midVal, maxVal) < depths[x, y]) {
                        Vector3 treePos = JigglePosition(new Vector3(x, depths[x, y] * zScale, y));
                        Instantiate(tree, treePos, Quaternion.identity);
                    }
                }
            }
        }
    }

    float CalculateMedian(float[] inArray) {
        Array.Sort(inArray);
        return inArray[inArray.Length / 2];
    }

    void CreateReeds()
    {
        for (int y = 2; y < landuseMap.height - 2; y++){
            for (int x = 2; x < landuseMap.width - 2; x++) {
                if (marsh[x, y]) {
                    if (UnityEngine.Random.Range(0, 100) < reedDensity) {
                        Vector3 reedPos = new Vector3(x, depths[x, y] * zScale, y);
                        Instantiate(reeds, reedPos, Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0)));
                    }
                } else {
                    if (depths[x, y] < midVal) {
                        if (UnityEngine.Random.Range(midVal, minVal) > depths[x, y]) {
                            Vector3 reedPos = JigglePosition(new Vector3(x, depths[x, y] * zScale, y));
                            Instantiate(reeds, reedPos, Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0)));
                        }
                    }
                }
            }
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



        // Assign colours to vertices
        for (int z = 0; z < heightZ; z++)
        {
            for (int x = 0; x < widthX; x++)
            {
                if (depths[x, z] < seaPos) {
                    colours[x + (z * widthX)] = seaCol;
                } else if (depths[x, z] - seaPos < coastSize) {
                    colours[x + (z * widthX)] = coastCol;
                } else if (depths[x, z] > seaPos + snowline) {
                    colours[x + (z * widthX)] = Color.white;
                } else if (river[x, z]) {
                    colours[x + (z * widthX)] = AddNoiseToColor(Color.blue);
                } else if (marsh[x, z]) {
                    colours[x + (z * widthX)] = marshCol;
                } else {
                    float vertHeight = Mathf.InverseLerp(minVal, maxVal, depths[x, z]);
                    colours[x + (z * widthX)] = AddNoiseToColor(gradient.Evaluate(vertHeight));
                }
            }
        }
        mesh.colors = colours;
    }

    void TogglePause()
    {
        if (pause) {
            pause = false;
        } else {
            pause = true;
        }
    }

    public float[,] GetDepths() {
        return depths;
    }

    public bool[,] GetRiver() {
        return river;
    }

    public bool[,] GetMarsh() {
        return marsh;
    }

    public float getZScale() {
        return zScale;
    }

    void Update()
    {
        if (!pause) {
//            UpdateMeshColors();
//            IncrementTime();
            seaPos = sls.GetGIAWaterHeight(year);
        }
//        if (Input.GetKeyDown(KeyCode.P)) TogglePause();
        if (quitBtn.WasPressedThisFrame()) UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
    }
}

