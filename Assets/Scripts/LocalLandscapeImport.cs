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
    string[,] headerText;

    int widthX = 256;
    int heightZ = 256;
    Vector3[] vertices;
    int[] triangles;
    Color[] colours;
    Mesh mesh;
    float seaPos;
    public float zScale;
    public Gradient gradient;
    [SerializeField] Gradient seaGradient;
    [SerializeField] float coastSize;
    [SerializeField] float minVal;
    [SerializeField] float maxVal;
    [SerializeField] float colourNoiseMin, colourNoiseMax;
    [SerializeField] float riverCutoff, marshCutoff;
    [SerializeField] int timeJumpAmt;
    [SerializeField] TMP_Text tundraTMP, wetlandTMP, grasslandTMP, woodlandTMP, intertidalTMP, riverTMP, seaTMP;
    [SerializeField] float featuresXoffsetInMetres, featuresYoffsetInMetres;
    [SerializeField] float featuresXsizeInMetres, featuresYsizeInMetres;
    [SerializeField] float origXcellSizeInMetres, origYcellSizeInMetres;
    [SerializeField] float origXtotalSizeInCells, origYtotalSizeInCells;
    [SerializeField] float importXcells, importYcells;


    public Color coastCol;
    public Color marshCol;
    public Color riverCol;


//    public Texture2D landuseMap;
    bool[,] river;
    bool[,] marsh;
    float[,] depths;
    bool[,] features;

    List<GameObject> allReeds = new List<GameObject>();
    List<GameObject> allTrees = new List<GameObject>();

    float maxTerrainForTundraCalc, minTerrainForTundraCalc;
    int tundraCount, wetlandCount, woodlandCount, coastCount, grasslandCount, riverCount, seaCount;


    public GameObject tree;
    public GameObject reeds;

    public int reedsPercent;
    public int treesPercent;

    public InputAction quitBtn;
    public InputAction controlsBtn;
    public InputAction timeJumpPlus;
    public InputAction timeJumpMinus;

    public GameObject controlScreen;
    public int updateFrequency;

    SeaLevelServer sls;
    TimeServer time;

    void OnEnable()
    {
        quitBtn.Enable();
        controlsBtn.Enable();
        timeJumpPlus.Enable();
        timeJumpMinus.Enable();
    }

    void OnDisable()
    {
        quitBtn.Disable();
        controlsBtn.Disable();
        timeJumpPlus.Disable();
        timeJumpMinus.Disable();
    }

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
        RefreshEnvironment();
    }

    void RefreshEnvironment()
    {
        ImportLocalSection();
        LoadFeatures();
        CreateMesh();
        UpdateMesh();
        GenerateRiverAndMarsh();
        CreateTrees();
        CreateReeds();
        UpdateMeshColors();
        UpdateEnvironmentPanel();
        AddMeshCollider();
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
                if (x == 0 && y == 0) {
                    maxTerrainForTundraCalc = DataStore.baseTerrain[thisX, thisY, time.GetYear() - 5000];
                    minTerrainForTundraCalc = maxTerrainForTundraCalc;
                }
                if (thisX == maxX || thisY == maxY) {
                    depths[x, y] = AddNoiseToDepths(DataStore.baseTerrain[thisX, thisY, time.GetYear() - 5000], x, y, minTerrainForTundraCalc, maxTerrainForTundraCalc);
                } else {
                    float xFactor = (float) xMod / (float) arrayAdjust;
                    float yFactor = (float) yMod / (float) arrayAdjust;

                    float topLeft = DataStore.baseTerrain[thisX, thisY + 1, time.GetYear() - 5000];
                    float topRight = DataStore.baseTerrain[thisX + 1, thisY + 1, time.GetYear() - 5000];
                    float bottomLeft = DataStore.baseTerrain[thisX, thisY, time.GetYear() - 5000];
                    float bottomRight = DataStore.baseTerrain[thisX + 1, thisY, time.GetYear() - 5000];

                    float calculatedHeight = Mathf.Lerp(Mathf.Lerp(bottomLeft, bottomRight, xFactor), Mathf.Lerp(topLeft, topRight, xFactor), yFactor);
                    depths[x, y] = AddNoiseToDepths(calculatedHeight, x, y, minTerrainForTundraCalc, maxTerrainForTundraCalc);
                    if (depths[x, y] > maxTerrainForTundraCalc) {
                        maxTerrainForTundraCalc = depths[x, y];
                    } else if (depths[x, y] < minTerrainForTundraCalc) {
                        minTerrainForTundraCalc = depths[x, y];
                    }
                }
            }
        }
    }

    void LoadFeatures()
    {

        string fileString;
        features = new bool[widthX, heightZ];
        fileString = ".\\UNPATHfeatures.asc";
        surfaceFile = new FileInfo (fileString);
        surfaceStream = surfaceFile.OpenText();
        string[] hdrArray;
        headerText = new string[2,6];
        int thisval = 0;
        char[] separators = new char[] { ' ', '\t', ',' };

        float clickedX = DataStore.selectedLocation.x;
        float clickedY = DataStore.selectedLocation.y;

        Debug.Log("Selected Location is " + DataStore.selectedLocation);

        //Read ESRI ASCII header
        for (int headline = 0; headline < 6; headline++)
        {
            inputLine = surfaceStream.ReadLine();
            hdrArray = inputLine.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
            headerText[0,headline] = hdrArray[0];
            headerText[1,headline] = hdrArray[1];
        }

        int totCols = int.Parse(headerText[1,0]);
        int totRows = int.Parse(headerText[1,1]);

        Debug.Log("Feature file Cols = " + totCols);
        Debug.Log("Feature file Rows = " + totRows);

        float Xcellwidth = (origXtotalSizeInCells * origXcellSizeInMetres) / importXcells;
        float Ycellwidth = (origYtotalSizeInCells * origYcellSizeInMetres) / importYcells;
        if (DataStore.selectedLocation.x * Xcellwidth < featuresXoffsetInMetres) {
            Debug.Log("Clicked point to the left of the data");
        } else if (DataStore.selectedLocation.x * Xcellwidth > featuresXoffsetInMetres + (totCols * featuresXsizeInMetres)) {
            Debug.Log("Clicked point to the right of the data");
        } else if (DataStore.selectedLocation.y * Ycellwidth < featuresYoffsetInMetres) {
            Debug.Log("Clicked point below the data");
        } else if (DataStore.selectedLocation.y * Ycellwidth > featuresYoffsetInMetres + (totRows * featuresYsizeInMetres)) {
            Debug.Log("Clicked point above the data");
        } else {
            float clickedXinMetres = DataStore.selectedLocation.x * Xcellwidth;
            float clickedYinMetres = DataStore.selectedLocation.y * Ycellwidth;

            int clickedXinCells = (int) ((clickedXinMetres - featuresXoffsetInMetres) / featuresXsizeInMetres);
            int clickedYinCells = (int) ((clickedYinMetres - featuresYoffsetInMetres) / featuresYsizeInMetres);

            int beginX = clickedXinCells - (widthX / 2);
            int beginY = clickedYinCells - (heightZ / 2);

            if (beginX < 0) {
                beginX = 0;
            } else if (beginX > totCols - widthX) {
                beginX = totCols - widthX;
            }
            if (beginY < 0) {
                beginY = 0;
            } else if (beginY > totRows - heightZ) {
                beginY = totRows - heightZ;
            }

            string[] readArray = new string[totCols];

            int xCount = 0;
            int yCount = heightZ - 1;
            bool lineUsed = false;
            for (int y = 0; y < totRows; y++)
            {
                inputLine = surfaceStream.ReadLine();
                xCount = 0;
                lineUsed = false;            
                readArray = inputLine.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
                for (int x = 0; x < totCols; x++)
                {
                    thisval = int.Parse(readArray[x]);

                    if (thisval > 1 && x >= beginX && x < beginX + widthX && y >= beginY && y < beginY + heightZ) {
                        features[xCount, yCount] = true;
                        xCount++;
                        lineUsed = true;
                        Debug.Log("Features " + xCount + "," + yCount + " is true");
                    }

                }
                if (lineUsed) {
                    yCount++;
                }
            }
        }
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

    void GenerateRiverAndMarsh()
    {
        river = new bool[widthX, heightZ];
        marsh = new bool[widthX, heightZ];
        riverCount = 0;
        woodlandCount = 0;
        tundraCount = 0;
        wetlandCount = 0;
        grasslandCount = 0;
        coastCount = 0;
        seaCount = 0;

        if (time.GetMaxSnowline() > minTerrainForTundraCalc) {
            float[,] flow = new float[widthX, heightZ];
            float[,] water = new float[widthX, heightZ];

            for (int x = 0; x < widthX; x++)
            {
                for (int y = 0; y < heightZ; y++)
                {
                    water[x, y] = 1.0f;
                }
            }

            for (int ticks = 0; ticks < (widthX / 2); ticks++)
            {
    //            Debug.Log("Tick# " + ticks);
                for (int x = 0; x < widthX; x++)
                {
                    for (int y = 0; y < heightZ; y++)
                    {
                        if (water[x, y] > 0.0f)
                        {
                            float lowestNeighbour = 9999.0f;
                            int lowestNeighbourX = 0;
                            int lowestNeighbourY = 0;
                            for (int nX = x - 1; nX <= x + 1; nX++)
                            {
                                for (int nY = y - 1; nY <= y + 1; nY++)
                                {
                                    if (nX >= 0 && nX < widthX && nY >= 0 && nY < heightZ)
                                    {
                                        if (depths[nX, nY] + water[nX, nY] < depths[x, y] + water[x, y] && depths[nX, nY] + water[nX, nY] < lowestNeighbour)
                                        {   
                                            lowestNeighbour = depths[nX, nY] + water[nX, nY];
                                            lowestNeighbourX = nX;
                                            lowestNeighbourY = nY;
                                        }
                                    }
                                }
                            }

                            if (lowestNeighbour < 1999.0f)
                            {
                                if (water[x, y] > (depths[x, y] + water[x, y]) - lowestNeighbour)
                                {
                                    float waterSlosh = (depths[x, y] + water[x, y] - lowestNeighbour) / 2.0f;
                                    water[x, y] = water[x, y] - waterSlosh;
                                    water[lowestNeighbourX, lowestNeighbourY] = water[lowestNeighbourX, lowestNeighbourY] + waterSlosh;
                                    flow[x, y] = flow[x, y] + waterSlosh;
                                } else {
                                    water[lowestNeighbourX, lowestNeighbourY] = water[lowestNeighbourX, lowestNeighbourY] + water[x, y];
                                    flow[x, y] = flow[x, y] + water[x, y];
                                    water[x, y] = 0;
                                }
                            }
                        }



                    }
                }

            }

            for (int x = 0; x < widthX; x++)
            {
                for (int y = 0; y < heightZ; y++)
                {
                    if (depths[x, y] < seaPos - coastSize) {
                        seaCount++;
                    } else if (depths[x, y] > time.GetMaxSnowline()) {
                        tundraCount++;
                    } else if (depths[x, y] > (seaPos - coastSize) && depths[x, y] < (seaPos + coastSize)) {
                        coastCount++;
                    } else if (flow[x, y] > riverCutoff)
                    {
                        river[x, y] = true;
                        riverCount++;
    //                    Debug.Log("Flow at " + x + "," + y + " is " + flow[x, y]);
                    } else if (water[x, y] > marshCutoff)
                    {
                        marsh[x, y] = true;
                        wetlandCount++;
    //                    Debug.Log("Water at " + x + "," + y + " is " + water[x, y]);
                    }
                }
            }

            
        } else {
            tundraCount = widthX * heightZ;
        }

    }

    void CreateTrees()
    {
        foreach(GameObject thisTree in allTrees)
        {
            Destroy(thisTree);
        }

        for (int x = 2; x < widthX - 2; x++) {
            for (int y = 2; y < heightZ - 2; y++) {
                if (UnityEngine.Random.Range(coastSize + 2, 20) < depths[x, y] && !river[x, y] && !marsh[x, y] && depths[x, y] < time.GetMaxSnowline()) {
                    if (treesPercent > 100) {
                        for (int t = 0; t < treesPercent / 100; t++) {
                            InstatiateTree(x, y);
                        }
                    }
                    if (UnityEngine.Random.Range(0, 100) < treesPercent % 100) {
                        InstatiateTree(x, y);
                        woodlandCount++;
                    }
                }
            }
        }
    }

    void CreateReeds()
    {
        foreach(GameObject thisReeds in allReeds)
        {
            Destroy(thisReeds);
        }
        
        for (int x = 2; x < widthX - 2; x++) {
            for (int y = 2; y < heightZ - 2; y++) {
                if (marsh[x, y]) {
                    if (reedsPercent > 100) {
                        for (int t = 0; t < reedsPercent / 100; t++) {
                            InstantiateReeds(x, y);
                        }
                    }
                    if (UnityEngine.Random.Range(0, 100) < reedsPercent % 100) {
                        InstantiateReeds(x, y);
                    }
                }
            }
        }
    }

    void UpdateMeshColors()
    {
        // Assign colours to vertices
        for (int z = 0; z < heightZ; z++)
        {
            for (int x = 0; x < widthX; x++)
            {
                if (depths[x, z] < seaPos) {
                    colours[x + (z * widthX)] = AddNoiseToColor(seaGradient.Evaluate((depths[x, z] - minTerrainForTundraCalc) / (maxTerrainForTundraCalc - minTerrainForTundraCalc)));
                } else if (river[x, z]) {
                    colours[x + (z * widthX)] = AddNoiseToColor(riverCol);
                } else if (depths[x, z] - seaPos < coastSize) {
                    colours[x + (z * widthX)] = AddNoiseToColor(CreateSands(coastCol, depths[x, z] - seaPos));
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

    void UpdateEnvironmentPanel()
    {
        int ungrassTotal = wetlandCount + riverCount + coastCount + woodlandCount + tundraCount + seaCount;
        float total = widthX * heightZ;
        grasslandCount = (int) total - ungrassTotal;
        wetlandTMP.text = "Wetland: " + GetPercentEnv(wetlandCount).ToString("0.00") + "%";
        riverTMP.text = "River: " + GetPercentEnv(riverCount).ToString("0.00") + "%";
        intertidalTMP.text = "Intertidal: " + GetPercentEnv(coastCount).ToString("0.00") + "%";
        woodlandTMP.text = "Woodland: " + GetPercentEnv(woodlandCount).ToString("0.00") + "%";
        seaTMP.text = "Sea: " + GetPercentEnv(seaCount).ToString("0.00") + "%";
        tundraTMP.text = "Tundra: " + GetPercentEnv(tundraCount).ToString("0.00") + "%";
        grasslandTMP.text = "Grassland: " + GetPercentEnv(grasslandCount).ToString("0.00") + "%";
    }

    float GetPercentEnv(int pCount)
    {
        return ((float) pCount / (widthX * heightZ)) * 100.0f;
    }

    void AddMeshCollider()
    {
        Destroy(GetComponent<MeshCollider>());
        MeshCollider thisMC = this.gameObject.AddComponent<MeshCollider>();
//        thisMC.sharedMesh = GetComponent<MeshFilter>().mesh;
        thisMC.sharedMesh = mesh;
    }

    void InstatiateTree(int pX, int  pY)
    {
        Vector3 treePos = JigglePosition(new Vector3(pX, depths[pX, pY] * zScale, pY));
        allTrees.Add(Instantiate(tree, treePos, Quaternion.identity));
    }

    void InstantiateReeds(int pX, int  pY)
    {
        Vector3 reedPos = JigglePosition(new Vector3(pX, depths[pX, pY] * zScale, pY));
        allReeds.Add(Instantiate(reeds, reedPos, Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0))));
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


    float AddNoiseToDepths(float depth, int pX, int pY, float pMin, float pMax)
    {
        float largeScale = 1.00f;
        float smallScale = 0.10f;
        float magnitudeLP = (pMax - pMin) / 2.0f;
        float magnitudeSP = (pMax - pMin) / 20.0f;
        float largeScaleNoise = depth + (Mathf.PerlinNoise((float) pX * largeScale, (float) pY * largeScale) * magnitudeLP - (magnitudeLP / 2));
        float smallScaleNoise = largeScaleNoise + Mathf.PerlinNoise((float) pX * smallScale, (float) pY * smallScale) * magnitudeSP - (magnitudeSP / 2);
        return smallScaleNoise;
    }


    Color AddNoiseToColor(Color inColor)
    {
        float rRand = UnityEngine.Random.Range(colourNoiseMin, colourNoiseMax);
        float gRand = UnityEngine.Random.Range(colourNoiseMin, colourNoiseMax);
        float bRand = UnityEngine.Random.Range(colourNoiseMin, colourNoiseMax);
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

    public int GetNumberOfCamps()
    {
        if (GetPercentEnv(tundraCount) + GetPercentEnv(seaCount) + GetPercentEnv(coastCount) > 95.0f) {
            return 0;
        } else {
            return 1;
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

    float CalculateMedian(float[] inArray) {
        Array.Sort(inArray);
        return inArray[inArray.Length / 2];
    }


    Color CreateSands(Color inColor, float seaDist)
    {
        float rAdj, gAdj, bAdj;
        float ripple = ColNormal((seaDist - (int) seaDist) / 2.0f);
//        Debug.Log(ripple);
        rAdj = ColNormal(inColor.r + ripple);
        gAdj = ColNormal(inColor.g);
        bAdj = ColNormal(inColor.b - ripple);
        return new Color(rAdj, gAdj, bAdj, 1);
    }

    public float GetDepths(int pX, int pY) {
        return depths[pX, pY];
    }

    public bool GetRiver(int pX, int pY) {
        return river[pX, pY];
    }

    public bool GetMarsh(int pX, int pY) {
        return marsh[pX, pY];
    }

    public bool GetTundra(int pX, int pY)
    {
        if (depths[pX, pY] > time.GetMaxSnowline()) {
            return true;
        } else {
            return false;
        }
    }

    public float getZScale() {
        return zScale;
    }

    public int GetLandscapeSize()
    {
        return widthX;
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

    void RefreshSeaPos()
    {
        seaPos = sls.GetGIAWaterHeight();
    }


    void Update()
    {
        RefreshSeaPos();
        if (time.GetDay() % updateFrequency == 0 && time.GetHour() == 1 && time.GetMinute() == 1) {
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
        if (timeJumpPlus.WasReleasedThisFrame()) {
            time.AdjustYear(timeJumpAmt);
//            Debug.Log("Year is now " + time.GetYear());
            RefreshSeaPos();
            RefreshEnvironment();
        }
        if (timeJumpMinus.WasReleasedThisFrame()) {
            time.AdjustYear(0 - timeJumpAmt);
//            Debug.Log("Year is now " + time.GetYear());
            RefreshSeaPos();
            RefreshEnvironment();
        }

    }
}

