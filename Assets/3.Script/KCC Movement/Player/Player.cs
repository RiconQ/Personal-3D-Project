using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private PlayerCharacter _playerCharacter;
    private Jump _jump;
    private Sliding _sliding;
    private LedgeClimb _ledgeClimb;
    private Grappling _grappling;
  
    [Header("Camera")]
    [SerializeField] private PlayerCamera _playerCamera;

    [Space]
    [SerializeField] private CameraSpring _cameraSpring;
    [SerializeField] private CameraLean _cameraLean;

    [Header("FX")]
    [Space]
    [SerializeField] private Volume _volume;
    [SerializeField] private StanceVignette _stanceVignette;
    [SerializeField] private ChromaticAberrationEffect _chromaticAberrationEffect;
    public ChromaticAberrationEffect ChromaticAberrationEffect => _chromaticAberrationEffect;

    private PlayerInputAction _inputAction;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _inputAction = new PlayerInputAction();
        _inputAction.Enable();

        //GetComponent
        GetMovementComponent();

        //Character Movement
        _playerCharacter.Initialize();
        _jump.Initialize();
        _sliding.Initialize();
        _ledgeClimb.Initialize();
        _grappling.Initialize();

        //Camera
        _playerCamera.Initialize(_playerCharacter.GetCameraTarget());

        //FX
        _cameraSpring.Initialize(_playerCharacter);
        _cameraLean.Initialize();
        _stanceVignette.Initialize(_volume.profile);
        _chromaticAberrationEffect.Initialize(_volume.profile);
    }

    private void GetMovementComponent()
    {
        _jump = GetComponentInChildren<Jump>();
        _sliding = GetComponentInChildren<Sliding>();
        _ledgeClimb = GetComponentInChildren<LedgeClimb>();
        _grappling = GetComponentInChildren<Grappling>();
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
            Crouch      = input.Crouch.WasPressedThisFrame() ?
                            ECrouchInput.Toggle : ECrouchInput.None,
            RightMouse = input.RightMouse.WasPressedThisFrame()
        };

        _ledgeClimb.UpdateLedgeClimb();
        _grappling.UpdateGrapple();

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

        _grappling.LateUpdateGrapple();
    }

    public void Teleport(Vector3 position)
    {
        _playerCharacter.SetPosition(position);
    }
}
