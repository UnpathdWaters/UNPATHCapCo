using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

[RequireComponent(typeof(MeshFilter))]
public class MenuLandscapeImport : MonoBehaviour
{
    protected FileInfo surfaceFile = null;
    protected StreamReader surfaceStream = null;
    protected string inputLine = " ";
    int widthX = 4096;
    int heightZ = 4096;
    float[,] depths;
    string[,] headerText;
    int totCols;
    int totRows;
    float noData;
    Vector3[] vertices;
    int[] triangles;
    Color[] colours;
    Mesh mesh;
    public Gradient gradient;
    float maxVal = -9999;
    float minVal = 9999;
    public float zScale = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        ImportData();
        CreateMesh();
        UpdateMesh();
    }

    void ImportData()
    {
        surfaceFile = new FileInfo ("Assets/surface.asc");
        surfaceStream = surfaceFile.OpenText();
        string[] hdrArray;
        depths = new float[widthX, heightZ];
        headerText = new string[2,6];
        float thisval = 0.0f;

        //Read ESRI ASCII header
        for (int headline = 0; headline < 6; headline++)
        {
            inputLine = surfaceStream.ReadLine();
            hdrArray = inputLine.Split(' ');
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
            readArray = inputLine.Split(' ');
            for (int x = 0; x < 4096; x++)
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

        // Assign colours to vertices
        for (int z = 0; z < heightZ; z++)
        {
            for (int x = 0; x < widthX; x++)
            {
                float vertHeight = Mathf.InverseLerp(minVal, maxVal, vertices[x + (z * widthX)].y);
                colours[x + (z * widthX)] = gradient.Evaluate(vertHeight);
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





    void UpdateMeshColors()
    {
        float vertHeight;
        // Assign colours to vertices
        for (int z = 0; z < heightZ; z++)
        {
            for (int x = 0; x < widthX; x++)
            {
                vertHeight = Mathf.InverseLerp(minVal, maxVal, vertices[x + (z * widthX)].y);
                colours[x + (z * widthX)] = gradient.Evaluate(vertHeight);
            }
        }
        mesh.colors = colours;
    }

    void Update()
    {
        UpdateMeshColors();
    }
   
}
