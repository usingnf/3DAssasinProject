using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public GameObject titlePanel;
    public GameObject stageUI;
    public GameObject stageConent;
    public GameObject optionUI;
    public Slider SESlider;
    public Slider MusicSlider;
    public Button stageButton;
    // Start is called before the first frame update
    void Start()
    {
        float SEVolume = PlayerPrefs.GetFloat("SEVolume");
        SESlider.value = SEVolume;
        float MusicVolume = PlayerPrefs.GetFloat("SEVolume");
        MusicSlider.value = MusicVolume;

        for (int i = 1; i <= 40; i++)
        {
            Button obj = Instantiate(stageButton, stageConent.transform);
            obj.transform.GetChild(0).GetComponent<Text>().text = i.ToString();
            int temp = i;
            obj.onClick.AddListener(() =>
            {
                PlayerPrefs.SetInt("Stage", temp);
                SceneManager.LoadScene("Stage" + temp.ToString());
            });
            
            //obj.onClick.AddListener(() => test(temp));
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
        stageUI.SetActive(true);
    }

    public void OptionOpen()
    {
        titlePanel.SetActive(false);
        optionUI.SetActive(true);
    }
    public void OptionClose()
    {
        titlePanel.SetActive(true);
        optionUI.SetActive(false);
    }

    public void SEChange(Slider slider)
    {
        Debug.Log(slider.value);
        PlayerPrefs.SetFloat("SEVolume", slider.value);
    }
    public void MusicChange(Slider slider)
    {
        PlayerPrefs.SetFloat("MusicVolume", slider.value);
    }
}
