using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class AnimalWithText : MonoBehaviour
{
    [SerializeField] GameObject floatingText;
    [SerializeField] InputAction showText;
    [SerializeField] string animalName;

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

    public void DisplayText()
    {
        var go = Instantiate(floatingText, transform.position, Quaternion.identity);
        TMP_Text thisText = go.GetComponent<TMP_Text>();
        thisText.text = animalName;
    }

    // Update is called once per frame
    void Update()
    {
        if (showText.WasReleasedThisFrame()) {
            DisplayText();
        }
    }
}
