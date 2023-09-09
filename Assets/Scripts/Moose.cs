using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moose : MonoBehaviour
{
    bool herdLeader, graze;
    int herdID;
    public float grazeChance;
    Vector2 destination;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Random.Range(0.0f, 1.0f) < grazeChance) {
            if (graze) {
                graze = false;
            } else {
                graze = true;
            }
        }
    }

    public void setLeader(bool pLeader)
    {
        herdLeader = pLeader;
    }

    public void setHerdID(int pID)
    {
        herdID = pID;
        Debug.Log("A moose from herd " + herdID + " is alive!");
    }

    public int getHerdID()
    {
        return herdID;
    }

    public bool getLeader()
    {
        return herdLeader;
    }

    public bool getGraze()
    {
        return graze;
    }

    public void setGraze(bool pGraze) {
        graze = pGraze;
    }

    public Vector2 getDestination() {
        return destination;
    }

    public void setDestination(Vector2 pDest) {
        destination = pDest;
    }
}
