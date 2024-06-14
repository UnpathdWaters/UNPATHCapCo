using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float destroyTime = 5f;
    TMP_Text thisText;

    // Start is called before the first frame update
    void Start()
    {
        thisText = gameObject.GetComponent<TMP_Text>();
        Destroy(gameObject, destroyTime);    
    }

    public void SetText(string pText)
    {
        thisText.text = pText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
