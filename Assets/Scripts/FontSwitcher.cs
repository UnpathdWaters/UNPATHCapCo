using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class FontSwitcher : MonoBehaviour
{
    [SerializeField] TMP_FontAsset font1;
    [SerializeField] TMP_FontAsset font2;
    bool fontUsed;
    TMP_Text[] thisText = new TMP_Text[4];
    public GameObject[] textFields = new GameObject[4];
    public InputAction fontToggle;

    // Start is called before the first frame update
    void Start()
    {
        for (int x = 0; x < 5; x++) {
            thisText[x] = textFields[x].GetComponent<TMP_Text>();
            thisText[x].font = font1;
        }
        fontUsed = true;
    }

    void OnEnable()
    {
        fontToggle.Enable();
    }

    void OnDisable()
    {
        fontToggle.Disable();
    }

    void SwitchFont()
    {
        if (fontUsed) {
            for (int x = 0; x < 5; x++) {
                thisText[x].font = font2;
            }
            fontUsed = false;
        } else {
            for (int x = 0; x < 5; x++) {
                thisText[x].font = font1;
            }
            fontUsed = true;
        }
    }

    void Update()
    {
        if (fontToggle.WasReleasedThisFrame())
        {
            SwitchFont();
        }
    }
}
