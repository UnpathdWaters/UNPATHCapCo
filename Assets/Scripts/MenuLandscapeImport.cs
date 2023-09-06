using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

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
    [SerializeField]
    GameObject arrow1;
    [SerializeField]
    GameObject arrow2;
    [SerializeField]
    GameObject arrow3;
    [SerializeField]
    GameObject arrow4;
    [SerializeField]
    GameObject arrow5;
    [SerializeField]
    GameObject arrow6;
    [SerializeField]
    GameObject arrow7;
    [SerializeField]
    GameObject glaciers20k;
    [SerializeField]
    GameObject glaciers17k;
    [SerializeField]
    GameObject glaciers15k;

    float seaPos;
    Color seaCol = new Color(0.0f, 0.0f, 0.9f, 1.0f);
    Color coastCol = new Color(0.8f, 0.8f, 0.0f, 1.0f);
    Color autumnTrees = new Color(0.9f, 0.2f, 0.0f, 1.0f);
    Color clickCol = new Color(0.9f, 0.0f, 0.9f, 1.0f);
    Color clickNeighbour = new Color(0.9f, 0.3f, 0.9f, 1.0f);

    enum Seasons { Winter, Spring, Summer, Autumn };
    Seasons season, lastSeason;
    float snowline;

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

    void ImportData()
    {
        surfaceFile = new FileInfo ("Assets/Terrain/surface600.csv");
        surfaceStream = surfaceFile.OpenText();
        string[] hdrArray;
        depths = new float[widthX, heightZ];
        trees = new bool[widthX, heightZ];
        treeSet = false;
        headerText = new string[2,6];
        float thisval = 0.0f;
        char[] separators = new char[] { ' ', '.', '\t', ',' };
        clickedPoint = new Vector2(0, 0);

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
                    EvaluateBaseColours();
                    break;
                case Seasons.Summer:
                    springImage.SetActive(false);
                    summerImage.SetActive(true);
                    autumnImage.SetActive(false);
                    winterImage.SetActive(false);
                    snowline = baseSnowline * 2;
                    EvaluateBaseColours();
                    break;
                case Seasons.Autumn:
                    springImage.SetActive(false);
                    summerImage.SetActive(false);
                    autumnImage.SetActive(true);
                    winterImage.SetActive(false);
                    snowline = baseSnowline;
                    EvaluateBaseColours();
                    break;
                case Seasons.Winter:
                    springImage.SetActive(false);
                    summerImage.SetActive(false);
                    autumnImage.SetActive(false);
                    winterImage.SetActive(true);
                    snowline = baseSnowline / 2;
                    EvaluateBaseColours();
                    break;
                default:
                    EvaluateBaseColours();
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
                if (depths[x, z] < seaPos) {
                    colours[x + (z * widthX)] = seaCol;
                } else if (x == clickedPoint.x && z == clickedPoint.y) {
                    colours[x + (z * widthX)] = clickCol;
                } else if (IsNeighbour(new Vector2(x, z), clickedPoint)) {
                    colours[x + (z * widthX)] = clickNeighbour;
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
                    if (depths[x, z] < (snowline + seaPos))
                    {
                        colours[x + (z * widthX)] = Color.Lerp(colours[x + (z * widthX)], Color.green, (timeThroughYear / 4.0f));
                    }
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
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (depths[(int) hit.point.x, (int) hit.point.z] > seaPos) {
                    clickedPoint.x = hit.point.x;
                    clickedPoint.y = hit.point.z;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            seaPos = -96.0f;
            baseSnowline = 100.0f;
            arrow1.SetActive(true);
            arrow2.SetActive(false);
            arrow3.SetActive(false);
            arrow4.SetActive(false);
            arrow5.SetActive(false);
            arrow6.SetActive(false);
            arrow7.SetActive(false);
            glaciers20k.SetActive(true);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
            EvaluateBaseColours();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            seaPos = -94.0f;
            baseSnowline = 125.0f;
            arrow1.SetActive(false);
            arrow2.SetActive(true);
            arrow3.SetActive(false);
            arrow4.SetActive(false);
            arrow5.SetActive(false);
            arrow6.SetActive(false);
            arrow7.SetActive(false);
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(true);
            glaciers15k.SetActive(false);
            EvaluateBaseColours();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            seaPos = -85.0f;
            baseSnowline = 150.0f;
            arrow1.SetActive(false);
            arrow2.SetActive(false);
            arrow3.SetActive(true);
            arrow4.SetActive(false);
            arrow5.SetActive(false);
            arrow6.SetActive(false);
            arrow7.SetActive(false);
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(true);
            EvaluateBaseColours();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            seaPos = -55.0f;
            baseSnowline = 300.0f;
            arrow1.SetActive(false);
            arrow2.SetActive(false);
            arrow3.SetActive(false);
            arrow4.SetActive(true);
            arrow5.SetActive(false);
            arrow6.SetActive(false);
            arrow7.SetActive(false);
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
            EvaluateBaseColours();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            seaPos = -33.0f;
            baseSnowline = 400.0f;
            arrow1.SetActive(false);
            arrow2.SetActive(false);
            arrow3.SetActive(false);
            arrow4.SetActive(false);
            arrow5.SetActive(true);
            arrow6.SetActive(false);
            arrow7.SetActive(false);
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
            EvaluateBaseColours();
        }
        if (Input.GetKeyDown(KeyCode.Alpha6)) {
            seaPos = -14.0f;
            baseSnowline = 450.0f;
            arrow1.SetActive(false);
            arrow2.SetActive(false);
            arrow3.SetActive(false);
            arrow4.SetActive(false);
            arrow5.SetActive(false);
            arrow6.SetActive(true);
            arrow7.SetActive(false);
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
            EvaluateBaseColours();
        }
        if (Input.GetKeyDown(KeyCode.Alpha7)) {
            seaPos = -5.0f;
            baseSnowline = 500.0f;
            arrow1.SetActive(false);
            arrow2.SetActive(false);
            arrow3.SetActive(false);
            arrow4.SetActive(false);
            arrow5.SetActive(false);
            arrow6.SetActive(false);
            arrow7.SetActive(true);
            glaciers20k.SetActive(false);
            glaciers17k.SetActive(false);
            glaciers15k.SetActive(false);
            EvaluateBaseColours();

        }
    }
}
