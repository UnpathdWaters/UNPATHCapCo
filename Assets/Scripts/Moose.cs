using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moose : MonoBehaviour
{
    bool herdLeader;
    int herdID;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
}
