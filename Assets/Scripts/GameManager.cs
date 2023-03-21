using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private PauseMenu pauseMenu;
    private bool isEscapePressed;
    private bool escapeKeyToggled;


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

        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);

        // ! Activate it only when game starts.
        pauseMenu = FindObjectOfType<PauseMenu>();
    }

    void Update()
    {
        if (isEscapePressed && !escapeKeyToggled)
        {
            escapeKeyToggled = true;
            bool isAlreadyPaused = pauseMenu.IsPaused;
            if (isAlreadyPaused)
            {
                pauseMenu.ResumeGame();
            }
            else
            {
                pauseMenu.PauseGame();
            }
        }
    }

    public void LoadMap()
    {
        StartCoroutine(LoadNewScene());
    }


    private IEnumerator LoadNewScene()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("basicScene1");
    }


    public void StopGameTimeFlow()
    {
        Time.timeScale = 0f;
        PlayerMovement.playerMovement.isPlayerTimeStopped = true;
    }

    public void ResumeGameTimeFlow()
    {
        Time.timeScale = 1f;
        PlayerMovement.playerMovement.isPlayerTimeStopped = false;
    }

    public void GetInputEscape(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isEscapePressed = true;
        }

        if (context.canceled)
        {
            escapeKeyToggled = false;
            isEscapePressed = false;
        }
    }
}
