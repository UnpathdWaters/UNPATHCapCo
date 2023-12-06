using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SLCsettings : MonoBehaviour
{

        TMP_InputField _5k;
        TMP_InputField _6k;
        TMP_InputField _7k;
        TMP_InputField _8k;
        TMP_InputField _9k;
        TMP_InputField _10k;
        TMP_InputField _11k;
        TMP_InputField _12k;
        TMP_InputField _13k;
        TMP_InputField _14k;
        TMP_InputField _15k;
        TMP_InputField _16k;
        TMP_InputField _17k;
        TMP_InputField _18k;
        TMP_InputField _19k;
        TMP_InputField _20k;

        float[] newSLC = new float[16];
        float[] oldSLC = new float[16];
        GameObject seaLevelServer;
        SeaLevelServer SLS;


    void Awake()
    {
        RefreshTextFields();
    }

    void RefreshTextFields()
    {
        seaLevelServer = GameObject.Find("SeaLevelServer");
        _5k = GameObject.Find("5k").GetComponent<TMP_InputField>();
        _6k = GameObject.Find("6k").GetComponent<TMP_InputField>();
        _7k = GameObject.Find("7k").GetComponent<TMP_InputField>();
        _8k = GameObject.Find("8k").GetComponent<TMP_InputField>();
        _9k = GameObject.Find("9k").GetComponent<TMP_InputField>();
        _10k = GameObject.Find("10k").GetComponent<TMP_InputField>();
        _11k = GameObject.Find("11k").GetComponent<TMP_InputField>();
        _12k = GameObject.Find("12k").GetComponent<TMP_InputField>();
        _13k = GameObject.Find("13k").GetComponent<TMP_InputField>();
        _14k = GameObject.Find("14k").GetComponent<TMP_InputField>();
        _15k = GameObject.Find("15k").GetComponent<TMP_InputField>();
        _16k = GameObject.Find("16k").GetComponent<TMP_InputField>();
        _17k = GameObject.Find("17k").GetComponent<TMP_InputField>();
        _18k = GameObject.Find("18k").GetComponent<TMP_InputField>();
        _19k = GameObject.Find("19k").GetComponent<TMP_InputField>();
        _20k = GameObject.Find("20k").GetComponent<TMP_InputField>();
        SLS = seaLevelServer.GetComponent<SeaLevelServer>();
        oldSLC = SLS.GetSLC();
        _5k.text = oldSLC[0].ToString();
        Debug.Log(_5k.text);
        _6k.text = oldSLC[1].ToString();
        _7k.text = oldSLC[2].ToString();
        _8k.text = oldSLC[3].ToString();
        _9k.text = oldSLC[4].ToString();
        _10k.text = oldSLC[5].ToString();
        _11k.text = oldSLC[6].ToString();
        _12k.text = oldSLC[7].ToString();
        _13k.text = oldSLC[8].ToString();
        _14k.text = oldSLC[9].ToString();
        _15k.text = oldSLC[10].ToString();
        _16k.text = oldSLC[11].ToString();
        _17k.text = oldSLC[12].ToString();
        _18k.text = oldSLC[13].ToString();
        _19k.text = oldSLC[14].ToString();
        _20k.text = oldSLC[15].ToString();
    }

    void OnDestroy()
    {
        newSLC[0] = float.Parse(_5k.text);
        newSLC[1] = float.Parse(_6k.text);
        newSLC[2] = float.Parse(_7k.text);
        newSLC[3] = float.Parse(_8k.text);
        newSLC[4] = float.Parse(_9k.text);
        newSLC[5] = float.Parse(_10k.text);
        newSLC[6] = float.Parse(_11k.text);
        newSLC[7] = float.Parse(_12k.text);
        newSLC[8] = float.Parse(_13k.text);
        newSLC[9] = float.Parse(_14k.text);
        newSLC[10] = float.Parse(_15k.text);
        newSLC[11] = float.Parse(_16k.text);
        newSLC[12] = float.Parse(_17k.text);
        newSLC[13] = float.Parse(_18k.text);
        newSLC[14] = float.Parse(_19k.text);
        newSLC[15] = float.Parse(_20k.text);
        SLS.SetSLC(newSLC);
        Debug.Log("Data written!");
    }
}
