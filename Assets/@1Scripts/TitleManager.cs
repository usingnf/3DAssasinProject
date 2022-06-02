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
    public GameObject keyOptionUI;
    public List<Text> keyOptionText;
    public Slider SESlider;
    public Slider MusicSlider;
    public Button stageButton;
    public bool isChangeKey = false;
    public Key key;
    private int keyType = 0;
    
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

    private void Update()
    {
        if (isChangeKey)
        {
            if(Input.anyKeyDown)
            {
                isChangeKey = false;
                for (int i = 0; i < 305; i++)
                {
                    if (Input.GetKeyDown((KeyCode)i))
                    {
                        SetKey(keyType, (KeyCode)i);
                    }
                }
            }
        }
    }

    public void SetKeyText()
    {
        keyOptionText[0].text = key.W.ToString();
        keyOptionText[1].text = key.A.ToString();
        keyOptionText[2].text = key.S.ToString();
        keyOptionText[3].text = key.D.ToString();
        keyOptionText[4].text = key.Space.ToString();
        keyOptionText[5].text = key.Q.ToString();
        keyOptionText[6].text = key.E.ToString();
        keyOptionText[7].text = key.C.ToString();
        keyOptionText[8].text = key.T.ToString();
        keyOptionText[9].text = key.Shift.ToString();
    }

    public void KeyOptionOpen()
    {
        titlePanel.SetActive(false);
        keyOptionUI.SetActive(true);
        SetKeyText();
    }

    public void KeyOptionClose()
    {
        titlePanel.SetActive(true);
        keyOptionUI.SetActive(false);
        isChangeKey = false;
    }

    public void SetKeyReady(int type)
    {
        isChangeKey = true;
        keyType = type;
    }

    public void SetKey(int type, KeyCode setKey)
    {
        if ((int)setKey >= 343 && (int)setKey <= 345)
            return; //Mouse Click Exception
        if (key.W == setKey)
            return;
        if (key.A == setKey)
            return;
        if (key.S == setKey)
            return;
        if (key.D == setKey)
            return;
        if (key.Space == setKey)
            return;
        if (key.Q == setKey)
            return;
        if (key.E == setKey)
            return;
        if (key.C == setKey)
            return;
        if (key.T == setKey)
            return;
        if (key.Shift == setKey)
            return;
        if (type == 1)
            key.W = setKey;
        else if (type == 2)
            key.A = setKey;
        else if (type == 3)
            key.S = setKey;
        else if (type == 4)
            key.D = setKey;
        else if (type == 5)
            key.Space = setKey;
        else if (type == 6)
            key.Q = setKey;
        else if (type == 7)
            key.E = setKey;
        else if (type == 8)
            key.C = setKey;
        else if (type == 9)
            key.T = setKey;
        else if (type == 10)
            key.Shift = setKey;
        
        SetKeyText();
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
