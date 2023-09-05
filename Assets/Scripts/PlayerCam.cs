using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float zoomAmount;
/*    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;*/

    // Start is called before the first frame update
    void Start()
    {
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
/*        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);*/

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

    }
}
