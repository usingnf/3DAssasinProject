using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }
    public Transform panel;
    public GameObject messageForm;
    int a = 0;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        //GameObject obj = null;
        //obj.transform.SetSiblingIndex(1);
        //obj.transform.parent = this.gameObject.transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            a++;
            CreateMessage(a.ToString());
        }
    }
    

    public void CreateMessage(string str)
    {
        Debug.Log(str);
        GameObject obj = Instantiate(messageForm, panel.transform);
        obj.transform.SetSiblingIndex(0);
        obj.transform.GetChild(0).GetComponent<Text>().text = str;
    }
}
