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
    float seaPos;
    public float zScale;
    public Gradient gradient;
    [SerializeField] float coastSize;

    public Color seaCol;
    public Color coastCol;
    public Color marshCol;


    public Texture2D landuseMap;
    bool[,] river;
    bool[,] marsh;
    float[,] depths;


    public GameObject tree;
    public GameObject reeds;

    public int reedDensity;
    public int treeDensity;

    public InputAction quitBtn;
    public InputAction controlsBtn;
    public GameObject controlScreen;
    public int updateFrequency;

    SeaLevelServer sls;
    TimeServer time;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        river = new bool[widthX, heightZ];
        marsh = new bool[widthX, heightZ];
        depths = new float[widthX, heightZ];
        sls = GameObject.Find("SeaLevelServer").GetComponent<SeaLevelServer>();
        time = GameObject.Find("TimeServer").GetComponent<TimeServer>();
        time.SetLocalMode(true);

        Debug.Log("Location is " + DataStore.selectedLocation);
        Debug.Log("Year is " + time.GetYear());

        seaPos = sls.GetGIAWaterHeight();
        ImportLocalSection();
//        CreateMesh();
//        UpdateMesh();
//        GenerateRiverAndMarsh();
//        CreateTrees();
//        CreateReeds();
//        UpdateMeshColors();
        Debug.Log("Local maxval is " + maxVal + " and minval is " + minVal + " and midVal is " + midVal);
    }

    void OnEnable()
    {
        quitBtn.Enable();
        controlsBtn.Enable();
    }

    void OnDisable()
    {
        quitBtn.Disable();
        controlsBtn.Disable();
    }

    void GenerateRiverAndMarsh()
    {
        for (int x = 1; x < widthX - 1; x++)
        {
            for (int y = 1; y < heightZ - 1; y++)
            {
                if (NumberOfLowerNeighbours(x, y) == 1)
                {
                    river[x,y] = true;
                } else if (NumberOfLowerNeighbours(x, y) == 0)
                {
                    marsh[x,y] = true;
                } 
            }
        }
    }

    int NumberOfLowerNeighbours(int pX, int pY)
    {
        int lowerNeighbours = 0;
        for (int x = pX - 1; x <= pX + 1; x++)
        {
            for (int y = pY - 1; y <= pY + 1; y++)
            {
                if (depths[x,y] < depths[pX, pY])
                {
                    lowerNeighbours++;
                }
            }
        }
        return lowerNeighbours;
    }

    void ImportLocalSection()
    {

        Debug.Log("widthX " + widthX + " heightZ" + heightZ);
        int arrayAdjust = (int) (widthX / DataStore.baseTerrain.GetLength(0));
        
        Debug.Log("arrayAdjust is " + arrayAdjust);
        
/*        for (int x = 0; x < widthX; x++) {
            for (int y = 0; y < heightZ; y++) {
                depths[x, y] = DataStore.baseTerrain[(int) x / arrayAdjust, (int) y / arrayAdjust];
            }
        }*/




/*        surfaceFile = new FileInfo (".\\SquareSelectableArea18764.asc");
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

        Debug.Log("selectedCol is " + selectedCol + " and Row is " + selectedRow + " so X goes from " + startX + "-" + endX + " and Y goes from" + startY + "-" + endY);

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
//                    Debug.Log(x + "," + z + "-" + thisval);
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
        midVal = CalculateMedian(tempArray);*/



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
                float vertHeight = Mathf.InverseLerp(minVal, maxVal, depths[x, z]);
                colours[x + (z * widthX)] = AddNoiseToColor(gradient.Evaluate(vertHeight));

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
                if (depths[x, y] > midVal && depths[x, y] > sls.GetGIAWaterHeight() + coastSize) {
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

    Color CreateSands(Color inColor, float seaDist)
    {
        float rAdj, gAdj, bAdj;
        float ripple = ColNormal((float) (((seaDist % 0.5f) - 0.25f) / 10.0f));
//        Debug.Log(ripple);
        rAdj = inColor.r + ripple;
        gAdj = inColor.g;
        bAdj = inColor.b;
        return new Color(rAdj, gAdj, bAdj, 1);
    }

    void UpdateMeshColors()
    {
        // Assign colours to vertices
        for (int z = 0; z < heightZ; z++)
        {
            for (int x = 0; x < widthX; x++)
            {
                if (depths[x, z] < seaPos) {
                    colours[x + (z * widthX)] = AddNoiseToColor(seaCol);
                } else if (depths[x, z] - seaPos < coastSize) {
                    colours[x + (z * widthX)] = CreateSands(coastCol, depths[x, z] - seaPos);
                } else if (depths[x, z] > seaPos + time.GetSnowline()) {
                    colours[x + (z * widthX)] = Color.white;
                } else if (river[x, z]) {
                    colours[x + (z * widthX)] = AddNoiseToColor(Color.blue);
                } else if (marsh[x, z]) {
                    colours[x + (z * widthX)] = marshCol;
                } else {
                    if (time.FirstDayOfSeason(updateFrequency)) {
                        float vertHeight = Mathf.InverseLerp(minVal, maxVal, depths[x, z]);
                        colours[x + (z * widthX)] = AddNoiseToColor(gradient.Evaluate(vertHeight));
                    } 
                }
            }
        }
        mesh.colors = colours;
//        Debug.Log("Updating mesh colours on day " + time.GetDay() + " of year " + time.GetYear() + " when snowline is " + time.GetSnowline());
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

    public float GetCoastSize() {
        return coastSize;
    }

    public float GetMidVal() {
        return midVal;
    }

    public bool IsSnow(int pX, int pY)
    {
        if (depths[pX, pY] > time.GetSnowline() + sls.GetGIAWaterHeight()) {
            return true;
        }
        return false;
    }

    public float GetLocationDepth(int pX, int pY)
    {
        return depths[pX, pY] * zScale;
    }


    void Update()
    {
        seaPos = sls.GetGIAWaterHeight();
        if (time.GetDay() % updateFrequency == 0) {
            UpdateMeshColors();
        }
        time.IncrementHour();
        if (quitBtn.WasPressedThisFrame()) {
            UnityEngine.SceneManagement.SceneManager.LoadScene("02MenuScene");
        }
        if (controlsBtn.IsPressed()) {
            controlScreen.SetActive(true);
        } else {
            controlScreen.SetActive(false);
        }

    }
}

