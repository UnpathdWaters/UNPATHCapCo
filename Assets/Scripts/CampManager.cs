using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampManager : MonoBehaviour
{

    public GameObject camp;
    int noOfCamps;
    List<GameObject> campList = new List<GameObject>();
    [SerializeField]
    GameObject landscapeManager;
    float[,] depths;
    bool depthsPop;
    float zScale;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!depthsPop) {
            LocalLandscapeImport land = landscapeManager.GetComponent<LocalLandscapeImport>();
            depths = land.GetDepths();
            zScale = land.getZScale();
            depthsPop = true;
        }

    }
}
