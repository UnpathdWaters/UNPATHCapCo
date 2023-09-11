using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MooseManager : MonoBehaviour
{
    public GameObject moose;
    int herds;
    List<GameObject> meese = new List<GameObject>();
    [SerializeField]
    GameObject landscapeManager;
    float[,] depths;
    bool depthsPop;
    Vector2[] leaderPos;
    float zScale;
    public float mooseSpeed;
    public float flockingDist;

    // Start is called before the first frame update
    void Start()
    {
        CreateHerd(6);
        CreateHerd(3);
        CreateHerd(8);
        CreateHerd(7);
    }

    public List<GameObject> GetMeese()
    {
        return meese;
    }

    // Update is called once per frame
    void Update()
    {
        if (!depthsPop) {
            LocalLandscapeImport land = landscapeManager.GetComponent<LocalLandscapeImport>();
            depths = land.GetDepths();
            zScale = land.getZScale();
            depthsPop = true;
            leaderPos = new Vector2[herds];
        }
        foreach(GameObject moose in meese) {
            Moose thisMoose = moose.GetComponent<Moose>();
            Vector2 moosePos = new Vector2(moose.transform.position.x, moose.transform.position.z);
            if (thisMoose.getLeader()) {
                if (thisMoose.getGraze()) {
                    if (Random.Range(0,100) < 3) {
                        Vector3 tempDest = randomMove(moose.transform.position);
                        moose.transform.LookAt(tempDest, Vector3.up);
                        moose.transform.position = Vector3.MoveTowards(moose.transform.position, tempDest, mooseSpeed * Time.deltaTime);
                    }
                } else {
//                    Debug.Log("Moose is travelling");
                    Vector2 thisDest = thisMoose.getDestination();
                    if (IsMooseThereYet(moose, thisDest)) {
//                        Debug.Log("...but moose is there");
                        thisMoose.setGraze(true);
                        thisMoose.setDestination(randomMooseDestination());
                    } else {
//                        Debug.Log("On his way from" + moose.transform.position + " to " + thisMoose.getDestination());
                        Vector3 tempDest = new Vector3(thisDest.x, depths[(int)thisDest.x, (int)thisDest.y] * zScale, thisDest.y);
                        moose.transform.LookAt(tempDest, Vector3.up);
                        moose.transform.position = Vector3.MoveTowards(moose.transform.position, tempDest, mooseSpeed * Time.deltaTime);
                    }
                }
                leaderPos[thisMoose.getHerdID()] = new Vector2(moose.transform.position.x, moose.transform.position.z);
            } else {
                if (Vector2.Distance(thisMoose.getPrecederLoc2(), moosePos) > flockingDist) {
                    Vector3 flockPos = thisMoose.getPrecederLoc3();
//                    Vector3 offsetPos = randomMooseOrigin(flockPos);
                    Vector3 offsetPos = flockPos;
                    moose.transform.LookAt(offsetPos, Vector3.up);
                    moose.transform.position = Vector3.MoveTowards(moose.transform.position, offsetPos, mooseSpeed * Time.deltaTime);
                } else if (Random.Range(0,100) < 10) {
                    Vector3 tempDest = randomMove(new Vector3(leaderPos[thisMoose.getHerdID()].x, depths[(int)leaderPos[thisMoose.getHerdID()].x, (int)leaderPos[thisMoose.getHerdID()].y] * zScale, leaderPos[thisMoose.getHerdID()].y));
                    moose.transform.LookAt(tempDest, Vector3.up);
                    moose.transform.position = Vector3.MoveTowards(moose.transform.position, tempDest, mooseSpeed * Time.deltaTime);
                }
            }
        }
    }

    bool IsMooseThereYet(GameObject pMoose, Vector2 pLoc) {
        Vector2 moosePos2D = new Vector2(pMoose.transform.position.x, pMoose.transform.position.z);
        if (Vector2.Distance(moosePos2D, pLoc) < 3.0f) {
            return true;
        } else {
            return false;
        }
    }

    Vector3 randomMove(Vector3 pInputVec) {
        Vector3 randomElement = new Vector3(Random.Range(-5.0f, 5.0f), 0, Random.Range(-5.0f, 5.0f));
        return pInputVec + randomElement;
    }

    Vector3 randomMooseOrigin(Vector3 pMooseOrig) {
        Vector3 randomElement = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
        return pMooseOrig + randomElement;
    }

    Vector2 randomMooseDestination() {
        int x = Random.Range(0, 512);
        int y = Random.Range(0, 512);
        Vector2 mooseReturn = new Vector2(x, y);
        return mooseReturn;
    }

    void CreateHerd(int noOfMeese) {
        Vector3 mooseOrigin = new Vector3(Random.Range(0, 511), 0, Random.Range(0,511));
        GameObject prevGO = null;
        for (int x = 0; x < noOfMeese; x++) {
            if (x == 0) {
                GameObject newMoose = Instantiate(moose, mooseOrigin, Quaternion.identity);
                Moose thisMoose = newMoose.GetComponent<Moose>();
                thisMoose.setLeader(true);
                thisMoose.setHerdID(herds);
                thisMoose.setDestination(randomMooseDestination());
                meese.Add(newMoose);
                prevGO = newMoose;
            } else {
                GameObject newMoose = Instantiate(moose, randomMooseOrigin(mooseOrigin), Quaternion.identity);
                Moose thisMoose = newMoose.GetComponent<Moose>();
                thisMoose.setLeader(false);
                thisMoose.setHerdID(herds);
                thisMoose.setPreceder(prevGO);
                meese.Add(newMoose);
                prevGO = newMoose;
            }
        }
        herds++;
    }
}
