using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimelineText : MonoBehaviour
{
    TimeServer time;
    TMP_Text thisText;
    // Start is called before the first frame update
    void Start()
    {
        time = GameObject.Find("TimeServer").GetComponent<TimeServer>();
        thisText = this.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        thisText.text = time.GetYear() + " years before present";
    }
}
