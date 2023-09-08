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

    // Start is called before the first frame update
    void Start()
    {
        CreateHerd(6);
    }

    // Update is called once per frame
    void Update()
    {
        if (!depthsPop) {
            LocalLandscapeImport land = landscapeManager.GetComponent<LocalLandscapeImport>();
            depths = land.GetDepths();
            depthsPop = true;
            leaderPos = new Vector2[herds];
        }
        foreach(GameObject moose in meese) {
            Moose thisMoose = moose.GetComponent<Moose>();
            Vector2 moosePos = new Vector2(moose.transform.position.x, moose.transform.position.z);
            if (thisMoose.getLeader()) {
                //Move leader
                leaderPos[thisMoose.getHerdID()] = moosePos;
            } else {
                if (Vector2.Distance(leaderPos[thisMoose.getHerdID()], moosePos) > 7.5f) {
                    //Move towards leader
                }
            }
        }
    }

    Vector3 randomMooseOrigin(Vector3 pMooseOrig) {
        Vector3 randomElement = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
        return pMooseOrig + randomElement;
    }

    void CreateHerd(int noOfMeese) {
        herds++;
        Vector3 mooseOrigin = new Vector3(Random.Range(0, 511), 0, Random.Range(0,511));
        for (int x = 0; x < noOfMeese; x++) {
            if (x == 0) {
                GameObject newMoose = Instantiate(moose, mooseOrigin, Quaternion.identity);
                Moose thisMoose = newMoose.GetComponent<Moose>();
                thisMoose.setLeader(true);
                thisMoose.setHerdID(herds);
                meese.Add(newMoose);
            } else {
                GameObject newMoose = Instantiate(moose, randomMooseOrigin(mooseOrigin), Quaternion.identity);
                Moose thisMoose = newMoose.GetComponent<Moose>();
                thisMoose.setLeader(false);
                thisMoose.setHerdID(herds);
                meese.Add(newMoose);
            }
        }
    }
}
