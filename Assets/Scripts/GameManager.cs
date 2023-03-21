using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
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
    }

    void Update()
    {
        if (isEscapePressed && !escapeKeyToggled)
        {
            escapeKeyToggled = true;
            bool isAlreadyPaused = PauseMenu.Instance.IsPaused;
            if (isAlreadyPaused)
            {
                PauseMenu.Instance.ResumeGame();
            }
            else
            {
                PauseMenu.Instance.PauseGame();
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
