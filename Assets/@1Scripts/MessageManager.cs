using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Singleton
public class MessageManager : MonoBehaviour
{
    //Singleton
    public static MessageManager Instance { get; private set; }
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private Transform viewPort;
    [SerializeField]
    private GameObject messageForm; //message prefab
    [SerializeField]
    private int count = 0;
    private int maxCount = 10; // 최대 메세지 개수
    [SerializeField]
    private Key key;
    void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if(Input.GetKeyDown(key.T))
        {
            canvas.enabled = !canvas.enabled;
        }
    }
    

    public void CreateMessage(string str)
    {
        count++;
        GameObject obj = Instantiate(messageForm, viewPort.transform);
        obj.transform.SetSiblingIndex(0);
        obj.transform.GetChild(0).GetComponent<Text>().text = str;
        if(viewPort.childCount > maxCount)
        {
            DeleteMessage(viewPort.childCount-1);
        }
    }

    public void DeleteMessage(int index)
    {
        if (index < 0)
            return;
        if (index >= viewPort.childCount)
            return;
        count--;
        Destroy(viewPort.GetChild(index).gameObject);
    }

}
