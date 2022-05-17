using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{ 
    None,
    Play,
    Stop,
    Finish,
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState gameState;
    public GameObject finishPanel;
    public GameObject pausePanel;
    public int stage = 0;
    
    void Awake()
    {
        stage = PlayerPrefs.GetInt("Stage");
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        gameState = GameState.Play;
        //Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(gameState == GameState.Play)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }
    }
    public void Finish()
    {
        Cursor.lockState = CursorLockMode.None;
        finishPanel.SetActive(true);
        Time.timeScale = 0.0f;
        gameState = GameState.Finish;
    }

    public void Restart()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Stage" + stage.ToString());
        gameState = GameState.Play;
        pausePanel.SetActive(false);
    }

    public void Next()
    {
        Time.timeScale = 1.0f;
        stage++;
        SceneManager.LoadScene("Stage" + stage.ToString());
        gameState = GameState.Play;
    }

    public void Exit()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("TitleScene");
        gameState = GameState.None;
    }

    public void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0.0f;
        gameState = GameState.Stop;
        pausePanel.SetActive(true);
        Debug.Log("pause");
    }

    public void Play()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1.0f;
        gameState = GameState.Play;
        pausePanel.SetActive(false);
    }
}
