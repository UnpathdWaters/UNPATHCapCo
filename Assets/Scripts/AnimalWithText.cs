using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimalWithText : MonoBehaviour
{
    [SerializeField] GameObject floatingText;
    [SerializeField] InputAction showText;
    Vector3 textOffset = new Vector3(0.0f, 100.0f, 0.0f);

    // Start is called before the first frame update
    void Start()
    {
    }

    void OnEnable()
    {
        showText.Enable();
    }

    void OnDisable()
    {
        showText.Disable();
    }

    void DisplayText()
    {
        Instantiate(floatingText, transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        if (showText.WasReleasedThisFrame()) {
            DisplayText();
        }
    }
}
