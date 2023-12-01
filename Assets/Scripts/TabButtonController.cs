using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabButtonController : MonoBehaviour
{
    Image tabImage;
    public Color selectedColor;
    public Color unselectedColor;

    // Start is called before the first frame update
    void Start()
    {
        tabImage = this.GetComponent<Image>();
    }

    public void SelectTab() {
        tabImage.color = selectedColor;
    }

    public void UnselectTab() {
        tabImage.color = unselectedColor;
    }
}
