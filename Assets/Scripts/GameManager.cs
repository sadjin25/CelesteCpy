using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private bool isEscapePressed;
    private bool escapeKeyToggled;

    void Start()
    {
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
