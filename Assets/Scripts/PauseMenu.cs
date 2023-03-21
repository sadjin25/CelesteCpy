using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private GameObject pauseMenu;
    public bool IsPaused
    {
        get;
        private set;
    }

    void Start()
    {
        pauseMenu = gameObject;
        pauseMenu.SetActive(false);
        DontDestroyOnLoad(gameObject);
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        GameManager.Instance.StopGameTimeFlow();
        IsPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        GameManager.Instance.ResumeGameTimeFlow();
        IsPaused = false;
    }
}
