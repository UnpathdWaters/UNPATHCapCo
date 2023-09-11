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
    [SerializeField]
    GameObject mooseManagerGO;
    MooseManager mooseManager;
    List<GameObject> meese;
    

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
            mooseManager = mooseManagerGO.GetComponent<MooseManager>();
            meese = mooseManager.GetMeese();
            for (int x = 0; x < noOfCamps; x++) {
                CreateCamp();
            }
            SetupCamps();
            depthsPop = true;
        }

        foreach (var aCamp in campList)
        {
            Camp thisCamp = aCamp.GetComponent<Camp>();
            float nearestdist = 999999.0f;
            Vector3 nearestLoc = new Vector2(0.0f, 0.0f);
            foreach (var aMoose in meese)
            {
                if (Vector3.Distance(aCamp.transform.position, aMoose.transform.position) < nearestdist) {
                    nearestdist = Vector3.Distance(aCamp.transform.position, aMoose.transform.position);
                    nearestLoc = new Vector2(aMoose.transform.position.x, aMoose.transform.position.z);
                }
            }
            thisCamp.SetNearestMoose(nearestLoc);
            if (thisCamp.GetFood() < 0) {
                campList.Remove(aCamp);
                Object.Destroy(aCamp);
                Debug.Log("Destroyed a camp");
            }            
        }
        if (campList.Count < noOfCamps) {
            CreateCamp();
        }

    }

    Vector2 FindNearestRiver(Vector2 pLoc)
    {
        float nearestDist = 99999.0f;
        Vector2 nearestLoc = new Vector2(0.0f,0.0f);
        for (int x = 0; x < depths.GetLength(0); x++) {
            for (int y = 0; y < depths.GetLength(1); y++) {
                if (river[x, y]) {
                    if (Vector2.Distance(pLoc, new Vector2(x, y)) < nearestDist) {
                        nearestDist = Vector2.Distance(pLoc, new Vector2(x, y));
                        nearestLoc = new Vector2(x, y);
                    }
                }
            }
        }
        return nearestLoc;
    }

    Vector2 FindNearestMarsh(Vector2 pLoc)
    {
        float nearestDist = 99999.0f;
        Vector2 nearestLoc = new Vector2(0.0f,0.0f);
        for (int x = 0; x < depths.GetLength(0); x++) {
            for (int y = 0; y < depths.GetLength(1); y++) {
                if (marsh[x, y]) {
                    if (Vector2.Distance(pLoc, new Vector2(x, y)) < nearestDist) {
                        nearestDist = Vector2.Distance(pLoc, new Vector2(x, y));
                        nearestLoc = new Vector2(x, y);
                    }
                }
            }
        }
        return nearestLoc;
    }

    void SetupCamps()
    {
        foreach (var camp in campList)
        {
            Vector2 campLoc = new Vector2(camp.transform.position.x, camp.transform.position.z);
            Camp thisCamp = camp.GetComponent<Camp>();
            thisCamp.SetNearestRiver(FindNearestRiver(campLoc));
            thisCamp.SetNearestMarsh(FindNearestMarsh(campLoc));
        }
    }

    bool CheckCampLoc(Vector2 pCampLoc)
    {
        for (int j = (int) pCampLoc.x - 2; j <= (int) pCampLoc.x + 2; j++)
        {
            for (int k = (int) pCampLoc.y -2; k <= (int) pCampLoc.y + 2; k++)
            {
                if (j >= 0 && j < depths.GetLength(0) && k >= 0 && k < depths.GetLength(1))
                {
                    if (river[j, k] || marsh[j, k]) {
                        return false;
                    }
                }

            }
        }
        return true;
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
        campLoc = GenerateCampLoc();
        while (!CheckCampLoc(campLoc)) {
            campLoc = GenerateCampLoc();
        }
        Vector3 campLoc3D = new Vector3(campLoc.x, depths[(int) campLoc.x, (int) campLoc.y] * zScale, campLoc.y);
        GameObject newCamp = Instantiate(camp, campLoc3D, Quaternion.identity);
        Debug.Log("New camp created at " + campLoc);
        campList.Add(newCamp);
    }
}
