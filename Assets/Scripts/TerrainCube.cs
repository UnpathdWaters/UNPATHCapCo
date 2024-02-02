using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCube : MonoBehaviour
{
    TerrainController terrainController;
    Material thisMat;

    void Start()
    {
        terrainController = GameObject.Find("Terrain Controller").GetComponent<TerrainController>();
        thisMat = this.gameObject.GetComponent<Renderer>().material;   
        ColourUpdate();
    }

    void ColourUpdate()
    {
        if (this.gameObject.transform.position.y < terrainController.GetSeaLevelPos())
        {
            thisMat.color = Color.blue;
        } else {
            thisMat.color = new Color(0.0f, 1.0f - (this.gameObject.transform.position.y - terrainController.GetMinHeight() / terrainController.GetHeightRange()), 0.0f);
        }
    }

    void Update()
    {

    }
}
