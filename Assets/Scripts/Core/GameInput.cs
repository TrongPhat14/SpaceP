using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnMenuButtonPressed;
    public event EventHandler OnUpButtonPressed;
    public event EventHandler OnLeftButtonPressed;
    public event EventHandler OnRightButtonPressed;

    private InputActions inputActions;
    private InputAction menuAction;
    private InputAction upAction;
    private InputAction leftAction;
    private InputAction rightAction;
    private InputAction movementAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        inputActions = new InputActions();
        CacheActions();
    }

    private void OnEnable()
    {
        if (inputActions == null)
        {
            return;
        }

        menuAction.performed += HandleMenuPerformed;
        upAction.performed += HandleUpPerformed;
        leftAction.performed += HandleLeftPerformed;
        rightAction.performed += HandleRightPerformed;
        inputActions.Enable();
    }

    private void OnDisable()
    {
        if (inputActions == null)
        {
            return;
        }

        menuAction.performed -= HandleMenuPerformed;
        upAction.performed -= HandleUpPerformed;
        leftAction.performed -= HandleLeftPerformed;
        rightAction.performed -= HandleRightPerformed;
        inputActions.Disable();
    }

    private void HandleMenuPerformed(InputAction.CallbackContext context)
    {
        OnMenuButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void HandleUpPerformed(InputAction.CallbackContext context)
    {
        OnUpButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void HandleLeftPerformed(InputAction.CallbackContext context)
    {
        OnLeftButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void HandleRightPerformed(InputAction.CallbackContext context)
    {
        OnRightButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool IsUpActionPressed()
    {
        return upAction != null && upAction.IsPressed();
    }

    public bool IsRightActionPressed()
    {
        return rightAction != null && rightAction.IsPressed();
    }

    public bool IsLeftActionPressed()
    {
        return leftAction != null && leftAction.IsPressed();
    }

    public Vector2 GetMovementInputVector2()
    {
        return movementAction != null ? movementAction.ReadValue<Vector2>() : Vector2.zero;
    }

    private void CacheActions()
    {
        menuAction = inputActions.Player.Menu;
        upAction = inputActions.Player.LanderUp;
        leftAction = inputActions.Player.LanderLeft;
        rightAction = inputActions.Player.LanderRight;
        movementAction = inputActions.Player.Movement;
    }
}
