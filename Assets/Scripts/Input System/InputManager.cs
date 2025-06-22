using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputSystem_Actions inputActions;

    private void Awake()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Enable();
        }
    }

    public static InputSystem_Actions GetInputActions()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Enable();
        }
        return inputActions;
    }

    public static InputSystem_Actions Actions => GetInputActions();

    public static void EnableInput() => inputActions?.Enable();
    public static void DisableInput() => inputActions?.Disable();
}
