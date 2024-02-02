using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    float maxHeight, minHeight;

    void Start()
    {
        
    }

    public void SetMaxHeight(float pMH)
    {
        maxHeight = pMH;
    }

    public void SetMinHeight(float pMH)
    {
        minHeight = pMH;
    }

    public float GetMaxHeight()
    {
        return maxHeight;
    }

    public float GetMinHeight()
    {
        return minHeight;
    }

    void Update()
    {
        
    }
}
