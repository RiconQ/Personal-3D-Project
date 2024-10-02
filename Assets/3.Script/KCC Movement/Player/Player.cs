using DG.Tweening.Core.Easing;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    public static Player instance;

    [Header("Player Movement")]
    [SerializeField] private PlayerCharacter _playerCharacter;
    public PlayerCharacter PlayerCharacter => _playerCharacter;
    private Jump _jump;
    private Sliding _sliding;
    private LedgeClimb _ledgeClimb;
    private ObjectThrow _objectThrow;
    private Dash _dash;
  
    [Header("Camera")]
    [SerializeField] private PlayerCamera _playerCamera;
    public PlayerCamera PlayerCamera => _playerCamera;

    [Space]
    [SerializeField] private CameraSpring _cameraSpring;
    [SerializeField] private CameraLean _cameraLean;

    [Header("Chain Dagger")]
    [SerializeField] private K_DaggerController _daggerController;
    public K_DaggerController DaggerController => _daggerController;

    [Header("FX")]
    [Space]
    [SerializeField] private Volume _volume;
    [SerializeField] private StanceVignette _stanceVignette;
    [SerializeField] private ChromaticAberrationEffect _chromaticAberrationEffect;
    public ChromaticAberrationEffect ChromaticAberrationEffect => _chromaticAberrationEffect;

    private PlayerInputAction _inputAction;

    [Header("Weapon")]
    [SerializeField]private K_WeaponHolder _weaponHolder;
    public K_WeaponHolder WeaponHolder => _weaponHolder;

    public Build_Pause pause;

    private void Awake()
    {
        instance = this;
    }

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
        _objectThrow.Initialize();
        _dash.Initialize();


        //Camera
        _playerCamera.Initialize(_playerCharacter.GetCameraTarget());

        //FX
        _cameraSpring.Initialize(_playerCharacter);
        _cameraLean.Initialize();
        _stanceVignette.Initialize(_volume.profile);
        _chromaticAberrationEffect.Initialize(_volume.profile);

        //Weapon Component
        _weaponHolder.Initialize();

        _daggerController.Initialize();

        K_PullableControl.instance.Initialize();
    }

    private void GetMovementComponent()
    {
        _jump = GetComponentInChildren<Jump>();
        _sliding = GetComponentInChildren<Sliding>();
        _ledgeClimb = GetComponentInChildren<LedgeClimb>();
        _objectThrow = GetComponentInChildren<ObjectThrow>();
        _dash = GetComponentInChildren<Dash>();
    }

    private void OnDestroy()
    {
        _inputAction.Dispose();
    }

    private void Update()
    {
        //Debug.Log(_inputAction.Player.Move.ReadValue<Vector2>());

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pause.PauseGame();
        }

        var input = _inputAction.Player;
        var deltaTime = Time.deltaTime;

        //Get Camera Input and Update rotation
        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
        _playerCamera.UpdateRotation(cameraInput);

        // Get Character input and update
        var characterInput = new CharacterInput
        {
            Rotation            = _playerCamera.transform.rotation,
            Move                = input.Move.ReadValue<Vector2>(),
            Jump                = input.Jump.IsPressed(),
            Crouch              = input.Crouch.WasPressedThisFrame() ?
                                    ECrouchInput.Toggle : ECrouchInput.None,
            LeftMouse           = input.LeftMouse.WasPressedThisFrame(),
            LeftMouseReleased   = input.LeftMouse.WasReleasedThisFrame(),
            RightMouse          = input.RightMouse.WasPressedThisFrame(),
            RightMouseReleased  = input.RightMouse.WasReleasedThisFrame(),
            LeftShift           = input.Shift.WasPressedThisFrame()

        };

        // Get Weapon Input and Update
        var controllerInput = new ControllerInput
        {
            Kick                = input.Kick.WasPressedThisFrame(),
            KickReleased        = input.Kick.WasReleasedThisFrame(),
            LeftMouse           = input.LeftMouse.WasPressedThisFrame(),
            LeftMousePressing   = input.LeftMouse.IsPressed(),
            LeftMouseReleased   = input.LeftMouse.WasReleasedThisFrame(),
            RightMouse          = input.RightMouse.WasPressedThisFrame(),
            RightMousePressing  = input.RightMouse.IsPressed(),
            RightMouseReleased  = input.RightMouse.WasReleasedThisFrame(),
            Crouch              = input.Crouch.WasPressedThisFrame(),
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

        //Weapon Update
        _weaponHolder.UpdateInput(controllerInput, deltaTime);
        _weaponHolder.UpdateController(deltaTime);

        //Chain Dagger
        _daggerController.UpdateInputDagger(controllerInput, deltaTime);
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

        _objectThrow.LateUpdateThrow();
    }

    public void Teleport(Vector3 position)
    {
        _playerCharacter.SetPosition(position);
    }
}
