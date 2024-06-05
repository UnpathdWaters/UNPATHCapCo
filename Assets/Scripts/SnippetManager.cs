using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SnippetManager : MonoBehaviour
{
    TMP_Text snippetText;
    int count = 0;
    string currentMessage;

    List<string> messages = new List<string>();

    void Start()
    {
        snippetText = GetComponent<TMP_Text>();
    }

    public void AddMessage(string pMessage)
    {
        messages.Add(pMessage);
    }

    public void ClearMessages()
    {
        messages.Clear();
    }

    public void CycleMessages()
    {
        if (messages.Count == 0) {
            currentMessage = "See if you can see the small camp within the landscape. It's got a fire to help you find it.";
        } else if (count >= messages.Count - 1) {
            count = 0;
            currentMessage = messages[count];
        } else {
            count++;
            currentMessage = messages[count];
        }
    }

    void Update()
    {
        snippetText.text = currentMessage;
    }
}
