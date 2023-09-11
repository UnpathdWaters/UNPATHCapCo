using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float zoomAmount;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.mouseScrollDelta.y > 0)
        {
            transform.position += transform.forward * zoomAmount;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            transform.position -= transform.forward * zoomAmount;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.Rotate(10.0f, 0.0f, 0.0f, Space.Self);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            transform.Rotate(-10.0f, 0.0f, 0.0f, Space.Self);
        }
        if (transform.position.y < 0.0f)
        {
            transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
        }

    }
}
