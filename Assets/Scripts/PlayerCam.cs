using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] float zoomSpeed;
    [SerializeField] float rotateSpeed;
    [SerializeField] InputAction zoomControls;
    [SerializeField] InputAction rotateControls;
    [SerializeField] float minX, maxX, minZ, maxZ;

    float zoomAmount, rotateAmount;
    float lowPoint = 0.0f;


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

    public void SetLowPoint(float pLowPoint)
    {
        lowPoint = pLowPoint;
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
        if (transform.position.y < lowPoint) {
            transform.position = new Vector3(transform.position.x, lowPoint, transform.position.z);
        }
        if (transform.position.x < minX) {
            transform.position = new Vector3(minX, transform.position.y, transform.position.z);
        } else if (transform.position.x > maxX) {
            transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
        }
        if (transform.position.z < minZ) {
            transform.position = new Vector3(transform.position.x, transform.position.y, minZ);
        } else if (transform.position.z > maxZ) {
            transform.position = new Vector3(transform.position.x, transform.position.y, maxZ);
        }

        transform.Rotate((rotateAmount * rotateSpeed), 0.0f, 0.0f, Space.Self);
    }
}
