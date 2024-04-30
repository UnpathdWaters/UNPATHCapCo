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
    int widthX = 256;
    int heightZ = 256;
    Vector3[] vertices;
    int[] triangles;
    Color[] colours;
    Mesh mesh;
    float seaPos;
    public float zScale;
    public Gradient gradient;
    [SerializeField] float coastSize;
    [SerializeField] float minVal;
    [SerializeField] float maxVal;

    public Color seaCol;
    public Color coastCol;
    public Color marshCol;


//    public Texture2D landuseMap;
    bool[,] river;
    bool[,] marsh;
    float[,] depths;


    public GameObject tree;
    public GameObject reeds;

    public int reedsPerPoint;
    public int treesPerPoint;

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
        CreateMesh();
        UpdateMesh();
//        GenerateRiverAndMarsh();
        CreateTrees();
        CreateReeds();
        UpdateMeshColors();
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

        depths = new float[widthX, heightZ];
        int arrayAdjust = (int) (widthX / (DataStore.baseTerrain.GetLength(0) - 1));
//        Debug.Log("ArrayAdjust is " + arrayAdjust);
        int thisX, thisY, xMod, yMod;
        int maxX = widthX / arrayAdjust;
        int maxY = heightZ / arrayAdjust;
        for (int x = 0; x < widthX; x++) {
            for (int y = 0; y < heightZ; y++) {
                thisX = x / arrayAdjust;
                thisY = y / arrayAdjust;
                xMod = x % arrayAdjust;
                yMod = y % arrayAdjust;
                if (thisX == maxX || thisY == maxY) {
                    depths[x, y] = AddNoiseToDepths(DataStore.baseTerrain[thisX, thisY], x, y);
                } else {
                    float xFactor = (float) xMod / (float) arrayAdjust;
                    float yFactor = (float) yMod / (float) arrayAdjust;

                    float topLeft = DataStore.baseTerrain[thisX, thisY + 1];
                    float topRight = DataStore.baseTerrain[thisX + 1, thisY + 1];
                    float bottomLeft = DataStore.baseTerrain[thisX, thisY];
                    float bottomRight = DataStore.baseTerrain[thisX + 1, thisY];

                    float calculatedHeight = Mathf.Lerp(Mathf.Lerp(bottomLeft, bottomRight, xFactor), Mathf.Lerp(topLeft, topRight, xFactor), yFactor);
                    depths[x, y] = AddNoiseToDepths(calculatedHeight, x, y);
                }
            }
        }
    }

    float AddNoiseToDepths(float depth, int pX, int pY)
    {
        float largeScale = 1.00f;
        float smallScale = 0.10f;
        float largeScaleNoise = depth + (Mathf.PerlinNoise((float) pX * largeScale, (float) pY * largeScale) * 10.0f - 5.0f);
        float smallScaleNoise = largeScaleNoise + Mathf.PerlinNoise((float) pX * smallScale, (float) pY * smallScale) * 10.0f - 5.0f;
        return smallScaleNoise;
//        return Mathf.PerlinNoise((float) pX * scale, (float) pY * scale) * 100.0f;
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
        for (int x = 2; x < widthX - 2; x++) {
            for (int y = 2; y < heightZ - 2; y++) {
                if (UnityEngine.Random.Range(coastSize + 2, 20) < depths[x, y] && !river[x, y] && !marsh[x, y]) {
                    for (int t = 0; t < treesPerPoint; t++) {
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

        for (int x = 2; x < widthX - 2; x++) {
            for (int y = 2; y < heightZ - 2; y++) {
                if (marsh[x, y]) {
                    for (int r = 0; r < reedsPerPoint; r++) {
                        Vector3 reedPos = JigglePosition(new Vector3(x, depths[x, y] * zScale, y));
                        Instantiate(reeds, reedPos, Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0)));
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
                } else if (river[x, z]) {
                    colours[x + (z * widthX)] = AddNoiseToColor(Color.blue);
                } else if (depths[x, z] > seaPos + time.GetSnowline()) {
                    colours[x + (z * widthX)] = Color.white;
                } else if (marsh[x, z]) {
                    colours[x + (z * widthX)] = marshCol;
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
        if (time.GetDay() % updateFrequency == 0 && time.GetHour() == 1 && time.GetMinute() == 1) {
            UpdateMeshColors();
        }
        time.IncrementMinute();
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

