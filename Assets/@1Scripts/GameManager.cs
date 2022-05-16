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
    public int stage = 0;
    
    void Awake()
    {
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.None)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.None;
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
        SceneManager.LoadScene("MainScene");
    }

    public void Next()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("MainScene");
    }
}
