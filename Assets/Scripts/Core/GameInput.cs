using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnMenuButtonPressed;
    public event EventHandler OnUpButtonPressed;
    public event EventHandler OnLeftButtonPressed;
    public event EventHandler OnRightButtonPressed;

    private InputActions inputActions;

    private void Awake()
    {
        Instance = this;
        inputActions = new InputActions();
        inputActions.Enable();
        inputActions.Player.Menu.performed += Menu_performed;
        inputActions.Player.LanderUp.performed += LanderUp_performed;
        inputActions.Player.LanderLeft.performed += LanderLeft_performed;
        inputActions.Player.LanderRight.performed += LanderRight_performed;
    }

    private void Menu_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnMenuButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void LanderUp_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnUpButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void LanderLeft_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnLeftButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void LanderRight_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnRightButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy()
    {
        inputActions.Player.Menu.performed -= Menu_performed;
        inputActions.Player.LanderUp.performed -= LanderUp_performed;
        inputActions.Player.LanderLeft.performed -= LanderLeft_performed;
        inputActions.Player.LanderRight.performed -= LanderRight_performed;
        inputActions.Disable();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool IsUpActionPressed()
    {
        return inputActions.Player.LanderUp.IsPressed();
    }

    public bool IsRightActionPressed()
    {
        return inputActions.Player.LanderRight.IsPressed();
    }

    public bool IsLeftActionPressed()
    {
        return inputActions.Player.LanderLeft.IsPressed();
    }

    public Vector2 GetMovementInputVector2()
    {
        return inputActions.Player.Movement.ReadValue<Vector2>();
    }

}
