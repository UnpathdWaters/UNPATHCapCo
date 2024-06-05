using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] float moveSpeed;
    public float minX, maxX, minZ, maxZ;

    public InputAction playerControls;

    Vector2 moveDirection;
    Rigidbody rb;

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }


    private void Update() {
        moveDirection = playerControls.ReadValue<Vector2>();
    }

    private void FixedUpdate() 
    {
        rb.velocity = new Vector3(moveDirection.x * moveSpeed, 0, moveDirection.y * moveSpeed);
//        rb.MovePosition(rb.position + new Vector3(moveDirection.x * moveSpeed, 0, moveDirection.y * moveSpeed));

        if (rb.position.x < minX) {
            rb.MovePosition(new Vector3(minX, rb.position.y, rb.position.z));
            Debug.Log("Exceeded minX which is " + minX);
        } else if (rb.position.x > maxX) {
            rb.MovePosition(new Vector3(maxX, rb.position.y, rb.position.z));
            Debug.Log("Exceeded maxX which is " + maxX);
        }
        if (rb.position.z < minZ) {
            rb.MovePosition(new Vector3(rb.position.x, rb.position.y, minZ));
            Debug.Log("Exceeded minZ which is " + minZ);
        } else if (rb.position.z > maxZ) {
            rb.MovePosition(new Vector3(rb.position.x, rb.position.y, maxZ));
            Debug.Log("Exceeded maxZ which is " + maxZ);
        }
        
    }

}
