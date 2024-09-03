using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private PlayerCharacter _playerCharacter;

    [Header("Camera")]
    [SerializeField] private PlayerCamera _playerCamera;

    [Space]
    [SerializeField] private CameraSpring _cameraSpring;
    [SerializeField] private CameraLean _cameraLean;

    [Header("FX")]
    [Space]
    [SerializeField] private Volume _volume;
    [SerializeField] private StanceVignette _stanceVignette;

    private PlayerInputAction _inputAction;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _inputAction = new PlayerInputAction();
        _inputAction.Enable();

        _playerCharacter.Initialize();
        _playerCamera.Initialize(_playerCharacter.GetCameraTarget());

        _cameraSpring.Initialize();
        _cameraLean.Initialize();
        _stanceVignette.Initialize(_volume.profile);
    }

    private void OnDestroy()
    {
        _inputAction.Dispose();
    }

    private void Update()
    {
        //Debug.Log(_inputAction.Player.Move.ReadValue<Vector2>());

        var input = _inputAction.Player;
        var deltaTime = Time.deltaTime;

        //Get Camera Input and Update rotation
        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
        _playerCamera.UpdateRotation(cameraInput);

        // Get Character input and update
        var characterInput = new CharacterInput
        {
            Rotation    = _playerCamera.transform.rotation,
            Move        = input.Move.ReadValue<Vector2>(),
            Jump        = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch      = input.Crouch.WasPressedThisFrame() ?
                            ECrouchInput.Toggle : ECrouchInput.None,
            GrapplingHook   = input.GrapplingHook.WasPressedThisFrame(),
            GrapplingSwing = input.GrapplingSwing.WasPressedThisFrame()
        };

        _playerCharacter.UpdateInput(characterInput, _inputAction.Player.Move.ReadValue<Vector2>());

        _playerCharacter.UpdateBody(deltaTime);

        #if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                Teleport(hit.point);
            }
        }
        #endif
    }

    private void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = _playerCharacter.GetCameraTarget();
        var state = _playerCharacter.GetState();

        _playerCamera.UpdatePosition(cameraTarget);
        _cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        _cameraLean.UpdateLean(deltaTime, state.Stance is EStance.Slide, state.Acceleration, cameraTarget.up);

        _stanceVignette.UpdateVignette(deltaTime, state.Stance);
    }

    public void Teleport(Vector3 position)
    {
        _playerCharacter.SetPosition(position);
    }
}
