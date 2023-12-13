using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour
{

    Vector2 destination;
    
    public void SetDestination(Vector2 pDest)
    {
        destination = pDest;
    }

    public Vector2 GetDestination()
    {
        return destination;
    }
}
