using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour
{

    Vector2 destination;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDestination(Vector2 pDest)
    {
        destination = pDest;
    }

    public Vector2 GetDestination()
    {
        return destination;
    }
}
