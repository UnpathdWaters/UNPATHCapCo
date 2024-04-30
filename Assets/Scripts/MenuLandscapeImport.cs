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
    int widthX = 512;
    int heightZ = 512;
    float[,,] depths;
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
    string[] glacierModeNames = new string[3] { "Minimum", "Optimal", "Maximum" };
    Mesh mesh;
    public Gradient gradient;
    float maxVal;
    float minVal;
    public float zScale = 0.1f;
    Vector2 clickedPoint;
    [SerializeField] float coastSize;
    [SerializeField] Camera cam;
    [SerializeField] GameObject glaciers20kmax;
    [SerializeField] GameObject glaciers19kmax;
    [SerializeField] GameObject glaciers18kmax;
    [SerializeField] GameObject glaciers17kmax;
    [SerializeField] GameObject glaciers16kmax;
    [SerializeField] GameObject glaciers15kmax;
    [SerializeField] GameObject glaciers20kopt;
    [SerializeField] GameObject glaciers19kopt;
    [SerializeField] GameObject glaciers18kopt;
    [SerializeField] GameObject glaciers17kopt;
    [SerializeField] GameObject glaciers16kopt;
    [SerializeField] GameObject glaciers15kopt;
    [SerializeField] GameObject glaciers20kmin;
    [SerializeField] GameObject glaciers19kmin;
    [SerializeField] GameObject glaciers18kmin;
    [SerializeField] GameObject glaciers17kmin;
    [SerializeField] GameObject glaciers16kmin;
    [SerializeField] GameObject glaciers15kmin;
    [SerializeField] int leftCol;
    [SerializeField] int rightCol;
    [SerializeField] int topRow;
    [SerializeField] int bottomRow;
    [SerializeField] int timeJumpAmt;
    [SerializeField] TMP_Text glacierText;
    TimeServer time;
    SeaLevelServer sls;

    public InputAction quitBtn;
    public InputAction loadSceneBtn;
    public InputAction timeControl;
    public InputAction timeJumpPlus;
    public InputAction timeJumpMinus;
    public InputAction controlsBtn;
    public InputAction glacierModeBtn;
    public float timeSpeed;
    public GameObject loadingScreen;
    public GameObject controlScreen;
    public float gradientAnchor;

    Color seaCol = new Color(0.0f, 0.0f, 0.9f, 1.0f);
    Color coastCol = new Color(0.8f, 0.8f, 0.0f, 1.0f);
    Color autumnTrees = new Color(0.9f, 0.2f, 0.0f, 1.0f);
    Color clickCol = new Color(0.9f, 0.0f, 0.9f, 1.0f);
    Color clickNeighbour = new Color(0.9f, 0.3f, 0.9f, 1.0f);
    Color boxCol = new Color(1.0f, 0.0f, 0.0f, 1.0f);

    bool quittable;
    int timeChangeAmount;
    int glacierMode = 1;


    // Start is called before the first frame update
    void Start()
    {
        sls = GameObject.Find("SeaLevelServer").GetComponent<SeaLevelServer>();
        time = GameObject.Find("TimeServer").GetComponent<TimeServer>();
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
//        DebugDepth();
        ImportData();
        InitTimeManagement();
        CreateMesh();
        UpdateMesh();
        quittable = false;
        updateGlacierModeText();
    }


    void OnEnable()
    {
        quitBtn.Enable();
        loadSceneBtn.Enable();
        timeControl.Enable();
        timeJumpPlus.Enable();
        timeJumpMinus.Enable();
        controlsBtn.Enable();
        glacierModeBtn.Enable();

    }

    void OnDisable()
    {
        quitBtn.Disable();
        loadSceneBtn.Disable();
        timeControl.Disable();
        timeJumpPlus.Disable();
        timeJumpMinus.Disable();
        controlsBtn.Disable();
        glacierModeBtn.Disable();
    }

    void ImportData()
    {
        string fileString;
        int fileCount = 0;
        depths = new float[16, widthX, heightZ];
        for (int t = 20; t > 4; t--)
        {
            maxVal = -9999.0f;
            minVal = 9999.0f;
            fileString = ".\\Proc" + t + ".asc";
            surfaceFile = new FileInfo (fileString);
            surfaceStream = surfaceFile.OpenText();
            string[] hdrArray;
            headerText = new string[2,6];
            float thisval = 0.0f;
            char[] separators = new char[] { ' ', '\t', ',' };

            //Read ESRI ASCII header
            for (int headline = 0; headline < 6; headline++)
            {
                inputLine = surfaceStream.ReadLine();
//                Debug.Log(fileString +  " " + inputLine);
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
                    if (thisval == noData) {
                        thisval = 0.0f;
                    }
                    depths[fileCount, xCount, zCount] = thisval;
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
            Debug.Log(fileString + " maxval is " + maxVal + " and minval is " + minVal);
            fileCount++;
        }


        trees = new bool[widthX, heightZ];
        treeSet = false;
        clickedPoint = new Vector2(0, 0);
        if (DataStore.subsequentRun)
        {
            cam.transform.position = DataStore.cameraPosition;
            cam.transform.rotation = DataStore.cameraRotation;
        }
    }

    float GetVertexDepth(int pX, int pY)
    {
        int lowerbound = 20 - (time.GetYear() / 1000);
        if (lowerbound == 15)
        {
            return depths[15, pX, pY];
        } else if (lowerbound == 0) {
            return depths[0, pX, pY];
        } else {
            return sls.UseInterpolation(depths[lowerbound - 1, pX, pY], depths[lowerbound, pX, pY]);
        }
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
                vertices[x + (z * widthX)] = new Vector3(x, GetVertexDepth(x, z) * zScale, z);

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
                float vertHeight = Mathf.InverseLerp(sls.GetGIAWaterHeight(), sls.GetGIAWaterHeight() + gradientAnchor, GetVertexDepth(x, z));
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
        Debug.Log("Year at Init is " + time.GetYear());
        time.SetLocalMode(false);
        SetGlacierVisibility();
        time.RefreshArrowIcon();
        time.SetArrowPosition();
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

    void cycleGlacierCoverage()
    {
        if (glacierMode < 2)
        {
            glacierMode++;
        } else {
            glacierMode = 0;
        }
        updateGlacierModeText();
    }

    void updateGlacierModeText()
    {
        glacierText.text = "Glacier Mode: " + glacierModeNames[glacierMode];
    }


    void ClearAllGlaciers()
    {
                glaciers20kmax.SetActive(false);
                glaciers19kmax.SetActive(false);
                glaciers18kmax.SetActive(false);
                glaciers17kmax.SetActive(false);
                glaciers16kmax.SetActive(false);
                glaciers15kmax.SetActive(false);
                glaciers20kopt.SetActive(false);
                glaciers19kopt.SetActive(false);
                glaciers18kopt.SetActive(false);
                glaciers17kopt.SetActive(false);
                glaciers16kopt.SetActive(false);
                glaciers15kopt.SetActive(false);
                glaciers20kmin.SetActive(false);
                glaciers19kmin.SetActive(false);
                glaciers18kmin.SetActive(false);
                glaciers17kmin.SetActive(false);
                glaciers16kmin.SetActive(false);
                glaciers15kmin.SetActive(false);
    }

    void SetGlacierVisibility()
    {
        ClearAllGlaciers();
        if (glacierMode == 2) {
            if (time.GetYear() == 20000) {
                glaciers20kmax.SetActive(true);
            } else if (time.GetYear() > 19000) {
                glaciers19kmax.SetActive(true);
            } else if (time.GetYear() > 18000) {
                glaciers18kmax.SetActive(true);
            } else if (time.GetYear() > 17000) {
                glaciers17kmax.SetActive(true);
            } else if (time.GetYear() > 16000) {
                glaciers16kmax.SetActive(true);
            } else if (time.GetYear() > 15000) {
                glaciers15kmax.SetActive(true);
            }
        } else if (glacierMode == 1) {
            if (time.GetYear() == 20000) {
                glaciers20kopt.SetActive(true);
            } else if (time.GetYear() > 19000) {
                glaciers19kopt.SetActive(true);
            } else if (time.GetYear() > 18000) {
                glaciers18kopt.SetActive(true);
            } else if (time.GetYear() > 17000) {
                glaciers17kopt.SetActive(true);
            } else if (time.GetYear() > 16000) {
                glaciers16kopt.SetActive(true);
            } else if (time.GetYear() > 15000) {
                glaciers15kopt.SetActive(true);
            }
        } else {
            if (time.GetYear() == 20000) {
                glaciers20kmin.SetActive(true);
            } else if (time.GetYear() > 19000) {
                glaciers19kmin.SetActive(true);
            } else if (time.GetYear() > 18000) {
                glaciers18kmin.SetActive(true);
            } else if (time.GetYear() > 17000) {
                glaciers17kmin.SetActive(true);
            } else if (time.GetYear() > 16000) {
                glaciers16kmin.SetActive(true);
            } else if (time.GetYear() > 15000) {
                glaciers15kmin.SetActive(true);
            }
        }
    }

    void UpdateMeshColors()
    {
        if (time.FirstDayOfSeason(1) && time.IsAutumn()) {
            treeSet = false;
        } else {
            treeSet = true;
        }
        // Assign colours to vertices
        for (int z = 0; z < heightZ; z++)
        {
            for (int x = 0; x < widthX; x++)
            {
                if (x == clickedPoint.x && z == clickedPoint.y) {
                    colours[x + (z * widthX)] = clickCol;
                } else if (IsNeighbour(new Vector2(x, z), clickedPoint)) {
                    colours[x + (z * widthX)] = clickNeighbour;
                } else if (GetVertexDepth(x, z) < sls.GetGIAWaterHeight()) {
                    colours[x + (z * widthX)] = seaCol;
                } else if (GetVertexDepth(x, z) - sls.GetGIAWaterHeight() < coastSize) {
                    colours[x + (z * widthX)] = coastCol;
                } else if (GetVertexDepth(x, z) > (time.GetSnowline() + sls.GetGIAWaterHeight())) {
                    colours[x + (z * widthX)] = Color.white;
                } else {

                        if (!treeSet) {
                            if (GetVertexDepth(x, z) < (time.GetSnowline() + sls.GetGIAWaterHeight()) && GetVertexDepth(x, z) > (sls.GetGIAWaterHeight() + (coastSize * 5))) {
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
                            colours[x + (z * widthX)] = Color.Lerp(basecol[x + (z * widthX)], autumnTrees, time.GetTimeThroughSeason());
                        } else if (GetVertexDepth(x, z) > (sls.GetGIAWaterHeight() + (coastSize * 5))) {
                            colours[x + (z * widthX)] = Color.Lerp(basecol[x + (z * widthX)], autumnTrees, (time.GetTimeThroughSeason() / 4.0f));
                        } else {
                            colours[x + (z * widthX)] = basecol[x + (z * widthX)];
                        }


                }
            }
        }
        mesh.colors = colours;
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

        if (quitBtn.WasReleasedThisFrame() && quittable) {
            float clickX = (clickedPoint.x - leftCol) / (rightCol - leftCol);
            float clickY = (clickedPoint.y - topRow) / (bottomRow - topRow);
            Vector2 clickedPointAsPercent = new Vector2(clickX, 1.0f - clickY);
            loadingScreen.SetActive(true);
            DataStore.selectedLocation = clickedPointAsPercent;
            DataStore.cameraPosition = cam.transform.position;
            DataStore.cameraRotation = cam.transform.rotation;
            DataStore.subsequentRun = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene("01IntroScene");
        }

        if (loadSceneBtn.WasReleasedThisFrame()) {
            Debug.Log("Depth of clicked point is " + GetVertexDepth((int) clickedPoint.x, (int) clickedPoint.y));

//            loadingScreen.SetActive(true);
//            DataStore.selectedLocation = clickedPointAsPercent;
            DataStore.cameraPosition = cam.transform.position;
            DataStore.cameraRotation = cam.transform.rotation;
            DataStore.subsequentRun = true;

            int baseTerrainX = 5;
            int baseTerrainY = 5;

            float[,] tempTerrain = new float[baseTerrainX, baseTerrainY];
            int clickx = (int) clickedPoint.x;
            int clicky = (int) clickedPoint.y;
            for (int x = 0; x < baseTerrainX; x++) {
                for (int y = 0; y < baseTerrainY; y++) {
                    tempTerrain[x, y] = GetVertexDepth(clickx - (baseTerrainX / 2) + x, clicky - (baseTerrainY / 2) + y);
//                    Debug.Log("tempTerrain " + x + "," + y + " is " + GetVertexDepth(clickx - 5 + x, clicky - 5 + y));
                }
            }
            DataStore.baseTerrain = tempTerrain;
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
        if (glacierModeBtn.WasReleasedThisFrame()) {
            cycleGlacierCoverage();
            SetGlacierVisibility();
        }

        if (timePeriodChanged) {
            Debug.Log("Year is now " + time.GetYear());
//            DebugDepth();
            SetGlacierVisibility();
            CreateMesh();
            UpdateMesh();
            UpdateMeshColors();
        }
        quittable = true;
    }
}
