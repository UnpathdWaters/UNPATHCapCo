using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class TabController : MonoBehaviour
{
    public GameObject[] tabs = new GameObject[3];
    public GameObject[] tabBody = new GameObject[3];
    public GameObject infoScreen;
    public InputAction tabRight;
    public InputAction tabLeft;
    public InputAction proceed;
    public InputAction quitBtn;
    public InputAction infoBtn;
    public InputAction colourBtn;
    public GameObject loadingScreen;
    public Sprite hcBackground;
    public Sprite background;
    public Color hcFontCol;
    public Color fontCol;
    public Color tabSelectCol;
    public Color tabUnselectCol;
    public Color tabHCSelectCol;
    public Color tabHCUnselectCol;

    int tabSelected;
    bool highContrast;

    // Start is called before the first frame update
    void Start()
    {
        tabSelected = 0;
        highContrast = false;
    }

    void OnEnable()
    {
        tabRight.Enable();
        tabLeft.Enable();
        proceed.Enable();
        quitBtn.Enable();
        infoBtn.Enable();
        colourBtn.Enable();
    }

    void OnDisable()
    {
        tabRight.Disable();
        tabLeft.Disable();
        proceed.Disable();
        quitBtn.Disable();
        infoBtn.Disable();
        colourBtn.Disable();
    }

    void ActivateTabBody(GameObject tabBody)
    {
        tabBody.SetActive(true);
        TMP_Text thisText = tabBody.transform.GetChild(0).GetComponent<TMP_Text>();
        Image thisBackground = tabBody.GetComponent<Image>();
        if (highContrast) {
            thisBackground.sprite = hcBackground;
            thisText.color = hcFontCol;
        } else {
            thisBackground.sprite = background;
            thisText.color = fontCol;
        }
    }

    void DeactivateTabBody(GameObject tabBody)
    {
        tabBody.SetActive(false);
    }

    void ActivateTab(GameObject tab)
    {
        TMP_Text thisText = tab.transform.GetChild(0).GetComponent<TMP_Text>();
        if (highContrast) {
            tab.GetComponent<Image>().color = tabHCSelectCol;
            thisText.color = hcFontCol;
        } else {
            tab.GetComponent<Image>().color = tabSelectCol;
            thisText.color = fontCol;
        }
    }

    void DeactivateTab(GameObject tab)
    {
        TMP_Text thisText = tab.transform.GetChild(0).GetComponent<TMP_Text>();
        if (highContrast) {
            tab.GetComponent<Image>().color = tabHCUnselectCol;
            thisText.color = hcFontCol;
        } else {
            tab.GetComponent<Image>().color = tabUnselectCol;
            thisText.color = fontCol;
        }
    }

    void RefreshTabs()
    {
        for (int x = 0; x < tabs.Length; x++)
        {
            if (x == tabSelected) {
                ActivateTab(tabs[x]);
                ActivateTabBody(tabBody[x]);
            } else {
                DeactivateTab(tabs[x]);
                DeactivateTabBody(tabBody[x]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
//        Debug.Log("Working");
        if (proceed.WasReleasedThisFrame()) {
            loadingScreen.SetActive(true);
            UnityEngine.SceneManagement.SceneManager.LoadScene("02MenuScene");
        }
        if (tabRight.WasReleasedThisFrame()) {
            if (tabSelected < 2) {
                tabSelected++;
                RefreshTabs();
            }
        }
        if (tabLeft.WasReleasedThisFrame()) {
            if (tabSelected > 0) {
                tabSelected--;
                RefreshTabs();
            }
        }
        if (quitBtn.WasReleasedThisFrame()) {
            Application.Quit();
        }
        if (infoBtn.IsPressed()) {
            infoScreen.SetActive(true);
        } else {
            infoScreen.SetActive(false);
        }
        if (colourBtn.WasReleasedThisFrame()) {
            if (highContrast) {
                highContrast = false;
            } else {
                highContrast = true;
            }
            RefreshTabs();
        }
    }
}
