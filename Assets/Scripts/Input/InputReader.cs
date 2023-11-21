using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "SOs/InputReader", order = 0)]
public class InputReader : ScriptableObject
{
    public UnityAction<Vector2> MoveEvent = delegate { };
    public UnityAction<bool> JumpEvent = delegate { };
    public UnityAction<bool> GrabEvent = delegate { };
    public UnityAction DashEvent = delegate { };

    GameInput gameInput;

    void OnEnable()
    {
        if (gameInput == null)
        {
            gameInput = new GameInput();
        }
    }

    void OnDisable()
    {
        DisableAllInput();
    }

    public void DisableAllInput()
    {
        gameInput.Keyboard.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }
        if (context.canceled)
        {
            MoveEvent?.Invoke(Vector2.zero);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpEvent?.Invoke(true);
        }

        if (context.canceled)
        {
            JumpEvent?.Invoke(false);
        }
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GrabEvent?.Invoke(true);
        }
        if (context.canceled)
        {
            GrabEvent?.Invoke(false);
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            DashEvent?.Invoke();
        }
    }
}
