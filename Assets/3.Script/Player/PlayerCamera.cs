using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Singletone CameraManager for Player
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensX;
    public float sensY;

    [Space]
    [SerializeField] private Transform _orientation;

    //Player Input System Action;
    [HideInInspector] public PlayerInputSystem playerInput;

    private float _xRotation;
    private float _yRotation;

    private void Start()
    {
        playerInput = new PlayerInputSystem();

        SetCursorVisible(false);
    }

    private void Update()
    {
        LookAround();
    }

    /// <summary>
    /// Method for Cursor Visible
    /// True for show cursor and unlock cursor move
    /// False for hide cursor and lock cursor move
    /// </summary>
    /// <param name="value"></param>
    public void SetCursorVisible(bool value)
    {
        Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = value;
    }

    /// <summary>
    /// Look Around Camera Get From Mouse input
    /// </summary>
    private void LookAround()
    {
        // Get value from New Inputsystem
        float mouseX = Mouse.current.delta.x.ReadValue() * Time.deltaTime * sensX;
        float mouseY = Mouse.current.delta.y.ReadValue() * Time.deltaTime * sensY;

        _xRotation += mouseX;
        _yRotation -= mouseY;

        // Clamp Mouse x Value with range(-90~90)
        _yRotation = Mathf.Clamp(_yRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(_yRotation, _xRotation, 0);
        _orientation.rotation = Quaternion.Euler(0, _xRotation, 0);
    }
}
