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
        float MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        MusicSlider.value = MusicVolume;
        float maxStage = PlayerPrefs.GetInt("MaxStage");

        for (int i = 1; i <= 40; i++)
        {
            Button button = Instantiate(stageButton, stageConent.transform);
            button.transform.GetChild(0).GetComponent<Text>().text = i.ToString();
            int temp = i;
            button.onClick.AddListener(() =>
            {
                PlayerPrefs.SetInt("Stage", temp);
                SceneManager.LoadScene("Stage" + temp.ToString());
            });
            if(i > maxStage+1)
            {
                button.interactable = false;

            }
        }
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

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }
}
