using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public GameObject InfoTab;
    public GameObject ControlTab;
    public GameObject SettingTab;
    public GameObject InfoBody;
    public GameObject ControlBody;
    public GameObject SettingBody;
    public InputAction tabRight;
    public InputAction tabLeft;
    public InputAction proceed;

    int tabSelected;

    // Start is called before the first frame update
    void Start()
    {
        tabSelected = 0;    
    }

    void OnEnable()
    {
        tabRight.Enable();
        tabLeft.Enable();
        proceed.Enable();
    }

    void OnDisable()
    {
        tabRight.Disable();
        tabLeft.Disable();
        proceed.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        bool change = false;
        Debug.Log("Working");
        if (proceed.WasReleasedThisFrame()) {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
        }
        if (tabRight.WasReleasedThisFrame()) {
            Debug.Log("Tabright!");
            if (tabSelected < 2) {
                tabSelected++;
                change = true;
                Debug.Log("Worked!");
            }
        }
        if (tabLeft.WasReleasedThisFrame()) {
            Debug.Log("Tableft!");
            if (tabSelected > 0) {
                tabSelected--;
                change = true;
                Debug.Log("Worked!");
            }
        }
        if (change) {
            if (tabSelected == 0) {
                InfoBody.SetActive(true);
                ControlBody.SetActive(false);
                SettingBody.SetActive(false);
            } else if (tabSelected == 1) {
                InfoBody.SetActive(false);
                ControlBody.SetActive(true);
                SettingBody.SetActive(false);
            } else if (tabSelected == 2) {
                InfoBody.SetActive(false);
                ControlBody.SetActive(false);
                SettingBody.SetActive(true);
            }
        }
    }
}
