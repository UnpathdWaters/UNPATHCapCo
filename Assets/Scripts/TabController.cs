using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public GameObject infoTab;
    public GameObject controlTab;
    public GameObject settingTab;
    public GameObject infoBody;
    public GameObject controlBody;
    public GameObject settingBody;
    public InputAction tabRight;
    public InputAction tabLeft;
    public InputAction proceed;
    public InputAction quitBtn;
    public GameObject loadingScreen;

    int tabSelected;
    TabButtonController infoTabBtn;
    TabButtonController controlTabBtn;
    TabButtonController settingTabBtn;

    // Start is called before the first frame update
    void Start()
    {
        tabSelected = 0;
        infoTabBtn = infoTab.GetComponent<TabButtonController>();
        controlTabBtn = controlTab.GetComponent<TabButtonController>();
        settingTabBtn = settingTab.GetComponent<TabButtonController>();

    }

    void OnEnable()
    {
        tabRight.Enable();
        tabLeft.Enable();
        proceed.Enable();
        quitBtn.Enable();
    }

    void OnDisable()
    {
        tabRight.Disable();
        tabLeft.Disable();
        proceed.Disable();
        quitBtn.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        bool change = false;
        Debug.Log("Working");
        if (proceed.WasReleasedThisFrame()) {
            loadingScreen.SetActive(true);
            UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
        }
        if (tabRight.WasReleasedThisFrame()) {
            if (tabSelected < 2) {
                tabSelected++;
                change = true;
            }
        }
        if (tabLeft.WasReleasedThisFrame()) {
            if (tabSelected > 0) {
                tabSelected--;
                change = true;
            }
        }
        if (quitBtn.WasReleasedThisFrame()) {
            Application.Quit();
        }
        if (change) {
            if (tabSelected == 0) {
                infoBody.SetActive(false);
                controlBody.SetActive(true);
                settingBody.SetActive(false);
                infoTabBtn.UnselectTab();
                controlTabBtn.SelectTab();
                settingTabBtn.UnselectTab();
            } else if (tabSelected == 1) {
                infoBody.SetActive(true);
                controlBody.SetActive(false);
                settingBody.SetActive(false);
                infoTabBtn.SelectTab();
                controlTabBtn.UnselectTab();
                settingTabBtn.UnselectTab();
            } else if (tabSelected == 2) {
                infoBody.SetActive(false);
                controlBody.SetActive(false);
                settingBody.SetActive(true);
                infoTabBtn.UnselectTab();
                controlTabBtn.UnselectTab();
                settingTabBtn.SelectTab();
            }
        }
    }
}
