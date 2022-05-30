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
    
    void Start()
    {
        //PlayerPrefs로 옵션 값 및 최대 스테이지 값 관리
        float SEVolume = PlayerPrefs.GetFloat("SEVolume");
        SESlider.value = SEVolume;
        float MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        MusicSlider.value = MusicVolume;
        float maxStage = PlayerPrefs.GetInt("MaxStage");

        //스테이지 버튼 생성
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

    public void StageClose()
    {
        stageUI.SetActive(false);
        titlePanel.SetActive(true);
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
        PlayerPrefs.SetFloat("SEVolume", slider.value);
    }
    public void MusicChange(Slider slider)
    {
        PlayerPrefs.SetFloat("MusicVolume", slider.value);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; //에디터 실행 중지
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }
}
