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
    private int SEASONLENGTH = 91;
    [SerializeField]
    GameObject springImage;
    [SerializeField]
    GameObject summerImage;
    [SerializeField]
    GameObject autumnImage;
    [SerializeField]
    GameObject winterImage;


    enum Seasons { Winter, Spring, Summer, Autumn };
    Seasons season;

    // Start is called before the first frame update
    void Start()
    {
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
        surfaceFile = new FileInfo ("Assets/surface600.csv");
        surfaceStream = surfaceFile.OpenText();
        string[] hdrArray;
        depths = new float[widthX, heightZ];
        headerText = new string[2,6];
        float thisval = 0.0f;
        char[] separators = new char[] { ' ', '.', '\t', ',' };

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

        // Assign colours to vertices
        for (int z = 0; z < heightZ; z++)
        {
            for (int x = 0; x < widthX; x++)
            {
                float vertHeight = Mathf.InverseLerp(minVal, maxVal, depths[x, z]);
                colours[x + (z * widthX)] = gradient.Evaluate(vertHeight);
                basecol[x + (z * widthX)] = gradient.Evaluate(vertHeight);

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
    }

    void IncrementTime()
    {
        day++;
        if (day > 365) {
            year--;
            day = 1;
        }
        season = (Seasons) (day / SEASONLENGTH);
        switch (season)
        {
            case Seasons.Spring:
                springImage.SetActive(true);
                summerImage.SetActive(false);
                autumnImage.SetActive(false);
                winterImage.SetActive(false);
                break;
            case Seasons.Summer:
                springImage.SetActive(false);
                summerImage.SetActive(true);
                autumnImage.SetActive(false);
                winterImage.SetActive(false);
                break;
            case Seasons.Autumn:
                springImage.SetActive(false);
                summerImage.SetActive(false);
                autumnImage.SetActive(true);
                winterImage.SetActive(false);
                break;
            case Seasons.Winter:
                springImage.SetActive(false);
                summerImage.SetActive(false);
                autumnImage.SetActive(false);
                winterImage.SetActive(true);
                break;
            default:
                break;
        }
    }




    void UpdateMeshColors()
    {
        float timeThroughSeason = (float) (day % SEASONLENGTH) /  (float) SEASONLENGTH;
//        Debug.Log("Time through season is " + timeThroughSeason);


        // Assign colours to vertices
        for (int z = 0; z < heightZ; z++)
        {
            for (int x = 0; x < widthX; x++)
            {
//                float vertHeight = Mathf.InverseLerp(minVal, maxVal, depths[x, z]);
//                colours[x + (z * widthX)] = gradient.Evaluate(vertHeight);


                switch (season)
                {
                    case Seasons.Spring:
                        colours[x + (z * widthX)] = Color.Lerp(basecol[x + (z * widthX)], Color.green, timeThroughSeason);
                        break;
                    case Seasons.Summer:
                        colours[x + (z * widthX)] = Color.Lerp(basecol[x + (z * widthX)], Color.yellow, timeThroughSeason);
                        break;
                    case Seasons.Autumn:
                        colours[x + (z * widthX)] = Color.Lerp(basecol[x + (z * widthX)], Color.red, timeThroughSeason);
                        break;
                    case Seasons.Winter:
                        colours[x + (z * widthX)] = Color.Lerp(basecol[x + (z * widthX)], Color.white, timeThroughSeason);
                        break;
                    default:
                        break;
                }
            }
        }
        mesh.colors = colours;
    }

    void Update()
    {
        UpdateMeshColors();
        IncrementTime();
    }
   
}
