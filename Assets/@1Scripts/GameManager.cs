using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Game상태
public enum GameState
{ 
    None,
    Play,
    Stop,
    Finish,
    Failed,
}

//Singleton
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState gameState;
    [SerializeField]
    private GameObject finishPanel;
    [SerializeField]
    private GameObject pausePanel;
    [SerializeField]
    private GameObject failedPanel;
    public int stage = 0;
    
    void Awake()
    {
        stage = PlayerPrefs.GetInt("Stage"); //PlayerPrefs로 현재/최대 스테이지 관리
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        gameState = GameState.Play;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameState == GameState.Play)
            {
                Pause();
            }
            else
            {
                if (gameState != GameState.Failed)
                {
                    Play();
                }
            }
        }
    }
    public void Finish()
    {
        Cursor.lockState = CursorLockMode.None;
        finishPanel.SetActive(true);
        Time.timeScale = 0.0f;
        gameState = GameState.Finish;
        if(stage >= PlayerPrefs.GetInt("MaxStage"))
        {
            PlayerPrefs.SetInt("MaxStage", stage);
        }
        MessageManager.Instance.CreateMessage("스테이지 클리어");
    }

    public void Restart()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Stage" + stage.ToString());
        gameState = GameState.Play;
        pausePanel.SetActive(false);
        failedPanel.SetActive(false);
        MessageManager.Instance.CreateMessage("스테이지 재시작");
    }

    public void Next()
    {
        Time.timeScale = 1.0f;
        stage++;
        PlayerPrefs.SetInt("Stage", stage);
        int sceneNum = SceneUtility.GetBuildIndexByScenePath("Stage" + stage.ToString());
        if (sceneNum <= 0)
        {
            Exit();
            return;
        }
        SceneManager.LoadScene("Stage" + stage.ToString());
        gameState = GameState.Play;
        MessageManager.Instance.CreateMessage("다음 스테이지");
    }

    public void Exit()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("TitleScene");
        gameState = GameState.None;
        MessageManager.Instance.CreateMessage("게임종료");
    }

    public void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0.0f;
        gameState = GameState.Stop;
        pausePanel.SetActive(true);
        MessageManager.Instance.CreateMessage("일시정지");
    }

    public void Play()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1.0f;
        gameState = GameState.Play;
        pausePanel.SetActive(false);
        MessageManager.Instance.CreateMessage("게임 시작");
    }

    public void Failed()
    {
        Cursor.lockState = CursorLockMode.None;
        gameState = GameState.Failed;
        failedPanel.SetActive(true);
        MessageManager.Instance.CreateMessage("스테이지 실패");
    }
}
