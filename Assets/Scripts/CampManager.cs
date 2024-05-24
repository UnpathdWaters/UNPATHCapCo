using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampManager : MonoBehaviour
{

    public GameObject camp;
    List<GameObject> campList = new List<GameObject>();
    [SerializeField] GameObject landscapeManager;
    [SerializeField] GameObject mooseManagerGO;
    MooseManager mooseManager;
    List<GameObject> meese;
    int lastYear = 99999;
    
    SeaLevelServer sls;
    LocalLandscapeImport land;
    TimeServer time;

    void Start()
    {
        sls = GameObject.Find("SeaLevelServer").GetComponent<SeaLevelServer>();
        time = GameObject.Find("TimeServer").GetComponent<TimeServer>();
        land = landscapeManager.GetComponent<LocalLandscapeImport>();
    }

    void Update()
    {

        if (time.GetYear() != lastYear) {
            foreach(GameObject thisCamp in campList) {
                Destroy(thisCamp);
            }
            for (int x = 0; x < land.GetNumberOfCamps(); x++) {
                CreateCamp();
            }
//            SetupCamps();
        }
        lastYear = time.GetYear();

/*        foreach (var aCamp in campList)
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
                Vector2 campLoc;
                campLoc = GenerateCampLoc();
                while (!CheckCampLoc(campLoc)) {
                    campLoc = GenerateCampLoc();
                }
                Vector3 campLoc3D = new Vector3(campLoc.x, depths[(int) campLoc.x, (int) campLoc.y] * zScale, campLoc.y);
                aCamp.transform.position = campLoc3D;
                thisCamp.ResetFood();
                Debug.Log("Moved a camp");
            }            
        }
        if (campList.Count < noOfCamps) {
            CreateCamp();
        }*/

    }

/*    Vector2 FindNearestRiver(Vector2 pLoc)
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
    }*/

/*    Vector2 FindNearestMarsh(Vector2 pLoc)
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
    }*/

/*    void SetupCamps()
    {
        foreach (var camp in campList)
        {
            Vector2 campLoc = new Vector2(camp.transform.position.x, camp.transform.position.z);
            Camp thisCamp = camp.GetComponent<Camp>();
            thisCamp.SetNearestRiver(FindNearestRiver(campLoc));
            thisCamp.SetNearestMarsh(FindNearestMarsh(campLoc));
        }
    }*/

    bool CheckCampLoc(Vector2 pCampLoc)
    {
        for (int j = (int) pCampLoc.x - 2; j <= (int) pCampLoc.x + 2; j++)
        {
            for (int k = (int) pCampLoc.y -2; k <= (int) pCampLoc.y + 2; k++)
            {
                if (j >= 0 && j < land.GetLandscapeSize() && k >= 0 && k < land.GetLandscapeSize())
                {
                    if (land.GetRiver(j, k) || land.GetMarsh(j, k) || land.GetDepths(j, k) < sls.GetGIAWaterHeight() + land.GetCoastSize() || land.GetTundra(j, k)) {
                        return false;
                    }
                }

            }
        }
        return true;
    }

    Vector2 GenerateCampLoc()
    {
        int x = Random.Range(0, land.GetLandscapeSize());
        int y = Random.Range(0, land.GetLandscapeSize());
        return new Vector2(x, y);
    }

    void CreateCamp() 
    {
        Vector2 campLoc;
        campLoc = GenerateCampLoc();
        while (!CheckCampLoc(campLoc)) {
            campLoc = GenerateCampLoc();
        }
        Vector3 campLoc3D = new Vector3(campLoc.x, land.GetDepths((int) campLoc.x, (int) campLoc.y) * land.getZScale(), campLoc.y);
        GameObject newCamp = Instantiate(camp, campLoc3D, Quaternion.identity);
        Debug.Log("New camp created at " + campLoc);
        campList.Add(newCamp);
    }
}
