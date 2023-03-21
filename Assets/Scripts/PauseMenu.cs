using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;
    private GameObject pauseMenu;
    public bool IsPaused
    {
        get;
        private set;
    }

    void Start()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }

        else
        {
            Instance = this;
        }

        pauseMenu = gameObject;
        pauseMenu.SetActive(false);
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        IsPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        IsPaused = false;
    }
}
