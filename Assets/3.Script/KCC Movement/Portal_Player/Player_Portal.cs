using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player_Portal : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private PlayerCharacter_Portal _playerCharacter;
    [SerializeField] private WallRunning_Portal _wallRunning;
    [SerializeField] private GrapplingSwing_Portal _graplingSwing;

    [Header("Portal Gun")]
    [SerializeField] private PortalGun _portalGun;

    [Header("Camera")]
    [SerializeField] private PlayerCamera_Portal _playerCamera;

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

        //Character Movement
        _playerCharacter.Initialize();
        _wallRunning.Initialize(_playerCharacter);
        _graplingSwing.Initialize(_playerCharacter);
        //Camera
        _playerCamera.Initialize(_playerCharacter.GetCameraTarget());

        //FX
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
            Rotation = _playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Dash = input.Dash.WasPressedThisFrame(),
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch = input.Crouch.WasPressedThisFrame() ?
                            ECrouchInput.Toggle : ECrouchInput.None,
            GrapplingSwing = input.GrapplingSwing.WasPressedThisFrame(),
            LeftPortal = input.LeftPortal.WasPressedThisFrame(),
            RIghtPortal = input.RightPortal.WasPressedThisFrame()
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

        _wallRunning.UpdateWallRun(deltaTime);
    }

    private void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = _playerCharacter.GetCameraTarget();
        var state = _playerCharacter.GetState();

        _playerCamera.UpdatePosition(cameraTarget);

        _stanceVignette.UpdateVignette(deltaTime, state.Stance);
    }

    public void Teleport(Vector3 position)
    {
        _playerCharacter.SetPosition(position);
    }
}
