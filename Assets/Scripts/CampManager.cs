using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampManager : MonoBehaviour
{

    public GameObject camp;
    public int noOfCamps;
    List<GameObject> campList = new List<GameObject>();
    [SerializeField]
    GameObject landscapeManager;
    float[,] depths;
    bool[,] river;
    bool[,] marsh;
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
            river = land.GetRiver();
            marsh = land.GetMarsh();
            depthsPop = true;
            CreateCamp();
        }

    }

    bool CheckCampLoc(Vector2 pCampLoc)
    {
        if (!river[(int) pCampLoc.x, (int) pCampLoc.y] && !marsh[(int) pCampLoc.x, (int) pCampLoc.y]) {
            return true;
        } else {
            return false;
        }

    }

    Vector2 GenerateCampLoc()
    {
        int x = Random.Range(0, depths.GetLength(0));
        int y = Random.Range(0, depths.GetLength(1));
        return new Vector2(x, y);
    }

    void CreateCamp() 
    {
        Vector2 campLoc;
        for (int x = 0; x < noOfCamps; x++) {
            campLoc = GenerateCampLoc();
            while (!CheckCampLoc(campLoc)) {
                campLoc = GenerateCampLoc();
            }
            Vector3 campLoc3D = new Vector3(campLoc.x, depths[(int) campLoc.x, (int) campLoc.y] * zScale, campLoc.y);
            GameObject newCamp = Instantiate(camp, campLoc3D, Quaternion.identity);
            Debug.Log("New camp created at " + campLoc);
//                Moose thisMoose = newMoose.GetComponent<Moose>();
//                thisMoose.setLeader(false);
//                thisMoose.setHerdID(herds);
            campList.Add(newCamp);

        }
    }
}
