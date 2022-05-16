using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public GameObject titlePanel;
    public GameObject stagePanel;
    public Button stageButton;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 1; i <= 40; i++)
        {
            Button obj = Instantiate(stageButton, stagePanel.transform);
            obj.transform.GetChild(0).GetComponent<Text>().text = i.ToString();
            Debug.Log(i);
            obj.onClick.AddListener(delegate 
            { 
                Debug.Log(i);
                int testint2 = i;
                SceneManager.LoadScene("Stage" + testint2.ToString());
            });
            //int testint = i;
            //obj.onClick.AddListener(() => test(testint));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void test(int i)
    {
        Debug.Log(i);
    }

    public void StartButton()
    {
        titlePanel.SetActive(false);
        stagePanel.SetActive(true);
    }
}
