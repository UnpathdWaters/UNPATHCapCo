using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    public float zoomSpeed;
    public float rotateSpeed;
    public InputAction zoomControls;
    public InputAction rotateControls;

    float zoomAmount, rotateAmount;


    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        zoomControls.Enable();
        rotateControls.Enable();
    }

    private void OnDisable()
    {
        zoomControls.Disable();
        rotateControls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        zoomAmount = zoomControls.ReadValue<float>();
        rotateAmount = rotateControls.ReadValue<float>();
    }

    void FixedUpdate()
    {
        transform.position += transform.forward * (zoomAmount * zoomSpeed);
        if (transform.position.y < 0) {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
        transform.Rotate((rotateAmount * rotateSpeed), 0.0f, 0.0f, Space.Self);
    }
}
