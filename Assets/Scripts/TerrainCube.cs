using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCube : MonoBehaviour
{
    TerrainController terrainController;

    void Start()
    {
        terrainController = GameObject.Find("Terrain Controller").GetComponent<TerrainController>();        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
