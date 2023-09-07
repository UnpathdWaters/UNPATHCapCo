using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

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

    public GameObject moose;
    public GameObject human;
    public GameObject tree;
    public GameObject reeds;

    Vector2 moosePos = new Vector2(300.0f, 300.0f);
    Vector2 humanPos = new Vector2(305.0f, 305.0f);
    Vector2 treePos = new Vector2(295.0f, 305.0f);

    public int reedDensity;

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
        ProcessMask();
        CreateTrees();
        CreateHumans();
        CreateMeese();
        CreateReeds();
    }

    void ImportData()
    {
        surfaceFile = new FileInfo ("Assets/Terrain/localsurface512.asc");
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

    void ProcessMask()
    {
        int riverPix = 0;
        int marshPix = 0;
        river = new bool[widthX, heightZ];
        marsh = new bool[widthX, heightZ];
        Color[] maskPixels = landuseMap.GetPixels();
        Debug.Log("maskPixels size is " + maskPixels.Length);
        Color thisCol;
        int useableX, useableY;
        float xFactor, yFactor;
        xFactor = (float) landuseMap.width / (float) widthX;
        yFactor = (float) landuseMap.height / (float) heightZ;
        Debug.Log("Xfac is " + xFactor + " and yfac is " + yFactor);
        for (int y = 0; y < landuseMap.height; y++){
            for (int x = 0; x < landuseMap.width; x++) {
                thisCol = maskPixels[x + (y * landuseMap.width)];
                useableX = (int) (x / xFactor);
                useableY = (int) (y / yFactor);
                if (thisCol.g < 0.5f && thisCol.a > 0.2f) {
                    river[useableX, useableY] = true;
                    Debug.Log("Pixel " + useableX + "," + useableY +" is river");
                    riverPix++;
                } else if (thisCol.g > 0.5f && thisCol.a > 0.2f) {
                    marsh[useableX, useableY] = true;
                    Debug.Log("Pixel " + useableX + "," + useableY +" is marsh");
                    marshPix++;
                } 
            }
        }
        Debug.Log(riverPix + " rivers and " + marshPix + " marshes");
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
        year = 20000;
        day = 1;
        season = Seasons.Winter;
        lastSeason = season;
        seaPos = -96.0f;
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

    void CreateTrees()
    {
        Vector3 treePos3D = new Vector3(treePos.x, depths[(int) treePos.x, (int) treePos.y] * zScale, treePos.y);
        Instantiate(tree, treePos3D, Quaternion.identity);
    }

    void CreateHumans()
    {
        Vector3 humanPos3D = new Vector3(humanPos.x, depths[(int) humanPos.x, (int) humanPos.y] * zScale, humanPos.y);
        Instantiate(human, humanPos3D, Quaternion.identity);
    }

    void CreateMeese()
    {
        Vector3 moosePos3D = new Vector3(moosePos.x, depths[(int) moosePos.x, (int) moosePos.y] * zScale, moosePos.y);
        Instantiate(moose, moosePos3D, Quaternion.identity);
    }

    void CreateReeds()
    {
        for (int y = 0; y < landuseMap.height; y++){
            for (int x = 0; x < landuseMap.width; x++) {
                if (marsh[x, y]) {
                    if (Random.Range(0, 100) < reedDensity) {
                        Vector3 reedPos = new Vector3(x, depths[x, y] * zScale, y);
                        Instantiate(reeds, reedPos, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
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
                    colours[x + (z * widthX)] = Color.blue;
                } else if (marsh[x, z]) {
                    colours[x + (z * widthX)] = marshCol;
                } else {
                    float vertHeight = Mathf.InverseLerp(minVal, maxVal, depths[x, z]);
                    colours[x + (z * widthX)] = gradient.Evaluate(vertHeight);
                }
            }
        }
        mesh.colors = colours;
    }

    void SetSeaPos()
    {
        float xPos = sea.transform.position.x;
        float zPos = sea.transform.position.z;
        Vector3 newPos = new Vector3(xPos, seaPos * zScale, zPos);
        sea.transform.position = newPos;
    }

    void TogglePause()
    {
        if (pause) {
            pause = false;
        } else {
            pause = true;
        }
    }

    void Update()
    {
        if (!pause) {
            UpdateMeshColors();
            IncrementTime();
            SetSeaPos();
        }
        if (Input.GetKeyDown(KeyCode.P)) TogglePause();
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }
}

