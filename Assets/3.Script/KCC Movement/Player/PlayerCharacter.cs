using KinematicCharacterController;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings.SplashScreen;

public enum ECrouchInput
{
    None,
    Toggle
}

public enum EStance
{
    Stand,
    Crouch,
    Slide
}

public enum EState
{
    Default,
    Dash
}

public struct CharacterState
{
    public bool Grounded;
    public EStance Stance;
    public Vector3 Velocity;
    public Vector3 Acceleration;
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Dash;
    public bool Jump;
    public bool JumpSustain;
    public ECrouchInput Crouch;
    public bool GrapplingSwing;
}

public struct RequestedInput
{
    public Quaternion Rotation;
    public Vector3 Movement;
    public bool Dash;
    public bool Jump;
    public bool SustainJump;
    public bool Crouch;
    public bool CrouchInAir;
    public bool GrapplingSwing;
}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    #region Variables

    [Header("Reference")]
    [SerializeField] private KinematicCharacterMotor _motor;
    [SerializeField] private Transform _root;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private PlayerCamera _playerCamera;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _gunTip;
    public Transform GunTip => _gunTip;
    [SerializeField] private LineRenderer _lr;

    [Header("Movement Reference")]
    [SerializeField] private WallRunning _wallRunning;
    [SerializeField] private WallCilmb _wallClimb;
    [SerializeField] private GrapplingSwing _grapplingSwing;

    [Space]
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 20f;
    [SerializeField] private float _crouchSpeed = 7f;

    [Space]
    [SerializeField] private float _walkResponse = 25f;
    [SerializeField] private float _crouchResponse = 20f;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed = 3f;
    [SerializeField] private float _dashTime = 0.1f;
    [SerializeField] private float _dashCooldownTime = 1f;
    private float _dashCooldownTimer;
    private EState _state;
    private float _timeSinceDash = 0f;
    private Vector3 _currentDashVelocity;

    [SerializeField] private float _exitWallTime = 0.2f;
    public float ExitWallTime => _exitWallTime;

    [Header("Wall Detection - Wall Climb")]
    [SerializeField] private float _wallClimbDetectionDistance = 1f;
    [SerializeField] private float _sphereCastRadius = 1f;
    [SerializeField] private float _maxWallLockAngle = 30f;
    [SerializeField] private float _minWallNormalAngleChange = 5f;

    [Header("Jump")]
    [SerializeField] private int _maxJumpCount = 2;
    [SerializeField] private float _jumpSpeed = 20f;
    [SerializeField] private float _coyoteTime = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float _jumpSustainGravity = 0.4f;
    [SerializeField] private float _gravity = -90f;

    [Header("Sliding")]
    [SerializeField] private float _slideStartSpeed = 25f;
    [SerializeField] private float _slideEndSpeed = 15;
    [SerializeField] private float _slideFriction = 0.8f;
    [SerializeField] private float _slideSteerAcceleration = 5f;
    [SerializeField] private float _slideGravity = -90f;

    [Header("Air Control")]
    [SerializeField] private float _airSpeed = 15f;
    [SerializeField] private float _airAcceleration = 70f;

    [Space]
    [Header("Height")]
    [SerializeField] private float _standHeight = 2f;
    [SerializeField] private float _crouchHeight = 1f;
    [SerializeField] private float _crouchHeightResponse = 15f;
    [Range(0f, 1f)]
    [SerializeField] private float _standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float _crouchCameraTargetHeight = 0.7f;

    //[Header("GrapplingSwing")]
    //[SerializeField] private LineRenderer _lr;
    //[SerializeField] private Transform _gunTip;
    //[Space]
    //[SerializeField] private LayerMask _whatIsGrappable;
    //[SerializeField] private float _maxGrappleDistance = 50f;
    //[SerializeField] private float _swingDelayTime = 0.25f;
    //[SerializeField] private float _swingCooldownTime = 0.25f;
    //[SerializeField] private float _swingForce = 2f;

    private CharacterState _currentState;
    private CharacterState _lastState;
    private CharacterState _tempState;

    // Input
    private RequestedInput _requestedInput;
    private Vector2 _moveInput;
    public Vector2 MoveInput => _moveInput;

    #region Code Variables
    // Jump
    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequest;
    private bool _ungroundedDueToJump;
    private int _remainJumpCount;

    [HideInInspector] public bool isExitingWall;
    [HideInInspector] public float exitWallTimer;


    private Collider[] _uncrouchOverlapResults;

    #region Grapple Swing
    //private Vector3 _swingPoint;
    //private float _swingCooldownTimer;
    //private bool _isGrappling;
    //private bool _isSwinging;
    //private Vector3 _characterToRopeAttachPoint;
    #endregion Grapple Swing

    #endregion Code Variables

    [Header("Debug")]
    public TMP_Text debugText;

    #endregion Variables

    public KinematicCharacterMotor Motor => _motor;

    public void Initialize()
    {
        _currentState.Stance = EStance.Stand;
        _lastState = _currentState;
        _uncrouchOverlapResults = new Collider[8];

        _motor.CharacterController = this;

        ResetVariables();
    }

    private void ResetVariables()
    {
        isExitingWall = false;
        //_isSwinging = false;
        //_lr.enabled = false;
        _remainJumpCount = _maxJumpCount;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
            debugText.gameObject.SetActive(!debugText.gameObject.activeSelf);
        //Debug
        string debugLog =
            $"Stance : {_currentState.Stance.ToString()}\n" +
            $"Grounded : {_currentState.Grounded}\n" +
            $"Velocity : {_motor.Velocity.magnitude}\n" +
            $"Jump : {_remainJumpCount} / {_maxJumpCount}\n" +
            $"FOV : {Camera.main.fieldOfView}";

        debugText.text = $"{debugLog}";
    }

    private void LateUpdate()
    {
        if (_grapplingSwing.IsGrappling || _grapplingSwing.IsSwing)
        {
            _lr.SetPosition(0, _gunTip.position);
        }

        Debug.DrawRay(transform.position, _motor.Velocity.normalized * 2, Color.yellow);
    }

    public void UpdateInput(CharacterInput input, Vector2 moveInput)
    {
        #region Move Vector2
        _moveInput = moveInput;
        #endregion

        #region Mouse
        _requestedInput.Rotation = input.Rotation;
        #endregion

        #region Movement
        // Take Vector2 Input -> Create 3D Movement Vector on XZ Plane
        _requestedInput.Movement = new Vector3(input.Move.x, 0f, input.Move.y);
        // Clamp to 1 length
        _requestedInput.Movement = Vector3.ClampMagnitude(_requestedInput.Movement, 1f);
        // Orient the input so it's relative to the direction the player is facing
        _requestedInput.Movement = input.Rotation * _requestedInput.Movement;
        #endregion

        #region Jump
        var wasRequestingJump = _requestedInput.Jump;
        _requestedInput.Jump = _requestedInput.Jump || input.Jump;
        if ((_requestedInput.Jump && !wasRequestingJump) && _remainJumpCount > 0)
        {
            _timeSinceJumpRequest = 0f;
        }

        _requestedInput.SustainJump = input.JumpSustain;
        #endregion

        #region Crouch
        var wasRequestingCrouch = _requestedInput.Crouch;
        _requestedInput.Crouch = input.Crouch switch
        {
            ECrouchInput.Toggle => !_requestedInput.Crouch,
            ECrouchInput.None => _requestedInput.Crouch,
            _ => _requestedInput.Crouch
        };

        if (_requestedInput.Crouch && !wasRequestingCrouch)
            _requestedInput.CrouchInAir = !_currentState.Grounded;
        else if (!_requestedInput.Crouch && wasRequestingCrouch)
            _requestedInput.CrouchInAir = false;
        #endregion

        #region Grappling Swing
        _requestedInput.GrapplingSwing = input.GrapplingSwing;
        if (_requestedInput.GrapplingSwing && !_motor.GroundingStatus.IsStableOnGround)
        {
            if (!_grapplingSwing.IsSwing && !_grapplingSwing.IsGrappling)
                _grapplingSwing.StartGrapplingSwing();
            else
                _grapplingSwing.StopGrapplingSwing();
        }
        #endregion

        #region Dash
        _requestedInput.Dash = input.Dash;
        if (_requestedInput.Dash && _dashCooldownTimer <= 0)
        {
            ChangeState(EState.Dash);
        }
        #endregion Dash
    }

    public void UpdateBody(float deltaTime)
    {
        var currentHeight = _motor.Capsule.height;
        var normalizedHeight = currentHeight / _standHeight;

        var cameraTargetHeight = currentHeight *
        (
            _currentState.Stance is EStance.Stand ? _standCameraTargetHeight : _crouchCameraTargetHeight
        );

        var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);

        _cameraTarget.localPosition = Vector3.Lerp
            (
                a: _cameraTarget.localPosition,
                b: new Vector3(0f, cameraTargetHeight, 0f),
                t: 1f - Mathf.Exp(-_crouchHeightResponse * deltaTime)
            );
        _root.localScale = Vector3.Lerp
            (
                a: _root.localScale,
                b: rootTargetScale,
                t: 1f - Mathf.Exp(-_crouchHeightResponse * deltaTime)
            );
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _currentState.Acceleration = Vector3.zero;
        switch (_state)
        {
            case EState.Default:
                // If player on Ground
                if (_motor.GroundingStatus.IsStableOnGround)
                {
                    if (_wallClimb.IsClimbing)
                    {
                        //Debug.Log("Ground WallClimb");
                        _wallClimb.ClimbingMovement(ref currentVelocity);
                    }
                    _timeSinceUngrounded = 0f;
                    _ungroundedDueToJump = false;

                    //snap the request movement direction to the angle of surface
                    // -> character is walking on Ground
                    var groundedMovement = _motor.GetDirectionTangentToSurface
                    (
                        direction: _requestedInput.Movement,
                        surfaceNormal: _motor.GroundingStatus.GroundNormal
                    ) * _requestedInput.Movement.magnitude;

                    // Start Sliding
                    {
                        var moving = groundedMovement.sqrMagnitude > 0f;
                        var crouching = _currentState.Stance is EStance.Crouch;
                        var wasStanding = _lastState.Stance is EStance.Stand;
                        var wasInAir = !_lastState.Grounded;
                        if (moving && crouching && (wasStanding || wasInAir))
                        {
                            _currentState.Stance = EStance.Slide;

                            /*
                             * When landing on stable ground the Character motor projects the velocity onto a flat ground plane
                             * See : KinematicCharacterMotor.HandleVelocityProjection()
                             * This is normally good, because under normal circumstances the player shouldn't slide when landing on the ground;
                             * In this case, we want the player to slide
                             * Reproject the last frame (falling) velocity onto the ground normal to slide
                             */
                            if (wasInAir)
                            {
                                currentVelocity = Vector3.ProjectOnPlane
                                (
                                    vector: _lastState.Velocity,
                                    planeNormal: _motor.GroundingStatus.GroundNormal
                                );
                            }

                            var effectiveSlideStartSpeed = _slideStartSpeed;
                            if (!_lastState.Grounded && !_requestedInput.CrouchInAir)
                            {
                                effectiveSlideStartSpeed = 0f;
                                _requestedInput.CrouchInAir = false;
                            }

                            var slideSpeed = Mathf.Max(effectiveSlideStartSpeed, currentVelocity.magnitude);
                            currentVelocity = _motor.GetDirectionTangentToSurface
                            (
                                direction: currentVelocity,
                                surfaceNormal: _motor.GroundingStatus.GroundNormal
                            ) * slideSpeed;
                        }
                    }

                    // Move
                    if (_currentState.Stance is EStance.Stand || _currentState.Stance is EStance.Crouch)
                    {
                        // Calculate the speed and responsiveness of movement based
                        //on the character's stance
                        var speed = _currentState.Stance is EStance.Stand ? _walkSpeed : _crouchSpeed;
                        var response = _currentState.Stance is EStance.Stand ? _walkResponse : _crouchResponse;

                        // Smooth move along the ground in that direction
                        var targetVelocity = groundedMovement * speed;
                        var moveVelocity = Vector3.Lerp
                        (
                            a: currentVelocity,
                            b: targetVelocity,
                            t: 1f - Mathf.Exp(-response * deltaTime)
                        );
                        _currentState.Acceleration = moveVelocity - currentVelocity;

                        currentVelocity = moveVelocity;
                    }
                    // Continue sliding
                    else
                    {

                        _playerCamera.DoFov(100f);

                        //Friction
                        currentVelocity -= currentVelocity * (_slideFriction * deltaTime);

                        // Slope
                        {
                            var force = Vector3.ProjectOnPlane
                            (
                                vector: -_motor.CharacterUp,
                                planeNormal: _motor.GroundingStatus.GroundNormal
                            ) * _slideGravity;
                            // In video 
                            //currentVelocity -= force * deltaTime;
                            // IDK why this not working
                            currentVelocity += force * deltaTime;
                        }

                        // Steer 
                        {
                            //Target velocity is the player's movement direction at the current speed
                            var currentSpeed = currentVelocity.magnitude;
                            var targetVelocity = groundedMovement * currentSpeed;
                            var steerVelocity = currentVelocity;
                            var steerForce = (targetVelocity - steerVelocity) * _slideSteerAcceleration * deltaTime;

                            //Add steer force, but clamp velocity so the slide speed doesnt increase due to direct movement input
                            steerVelocity += steerForce;
                            steerVelocity = Vector3.ClampMagnitude(steerVelocity, currentSpeed);

                            _currentState.Acceleration = (steerVelocity - currentVelocity) / deltaTime;

                            currentVelocity = steerVelocity;
                        }

                        //Stop
                        if (currentVelocity.magnitude < _slideEndSpeed)
                            _currentState.Stance = EStance.Crouch;
                    }
                }
                // character is in air
                else
                {
                    if (_wallRunning.IsWallRunning)
                    {
                        _wallRunning.WallRunningMovement(ref currentVelocity);
                    }
                    else if (_wallClimb.IsClimbing)
                    {
                        //Debug.Log("Air WallClimb");
                        _wallClimb.ClimbingMovement(ref currentVelocity);
                    }
                    else
                    {
                        if (_motor.Velocity.magnitude > 50)
                        {
                            _playerCamera.DoFov(100f);
                        }
                        else
                        {
                            _playerCamera.DoFov(80f);
                        }
                        _timeSinceUngrounded += deltaTime;

                        // Air Move
                        if (_requestedInput.Movement.sqrMagnitude > 0f)
                        {
                            // Requested movement projected onto movement plane (magnitude preserved)
                            var planarMovement = Vector3.ProjectOnPlane
                            (
                                vector: _requestedInput.Movement,
                                planeNormal: _motor.CharacterUp
                            ) * _requestedInput.Movement.magnitude;

                            // current velocity on movement plane
                            var currentPlanarVelocity = Vector3.ProjectOnPlane
                            (
                                vector: currentVelocity,
                                planeNormal: _motor.CharacterUp
                            );

                            //Calculate movement force;
                            // Will be changed depending on current velocity
                            var movementForce = planarMovement * _airAcceleration * deltaTime;

                            // if moving slower than the max air speed, treat movementForce as a simple steering force
                            if (currentPlanarVelocity.magnitude < _airSpeed)
                            {
                                //Add it to the current planer velocity for a target velocity
                                var targetPlanerVelocity = currentPlanarVelocity + movementForce;

                                //Limit target velocity to air speed
                                targetPlanerVelocity = Vector3.ClampMagnitude(targetPlanerVelocity, _airSpeed);

                                //Steer toward current velocity
                                movementForce = targetPlanerVelocity - currentPlanarVelocity;
                            }
                            //Otherwise, nerf the movementForce when it is in the direction of the current planer velocity
                            //to prevent accelerating further beyond the max air speed
                            else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                            {
                                //Project movement force onto the plane whose normal is the current planar velocity
                                var constrainedMovementForce = Vector3.ProjectOnPlane
                                (
                                    vector: movementForce,
                                    planeNormal: currentPlanarVelocity.normalized
                                );
                                movementForce = constrainedMovementForce;
                            }

                            //Prevent air-climbing steep slope
                            if (_motor.GroundingStatus.FoundAnyGround)
                            {
                                //if moving in the same direction as the resultant velocity
                                if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f)
                                {
                                    //Calculate Obstruction normal
                                    var obstructionNormal = Vector3.Cross
                                    (
                                        _motor.CharacterUp,
                                        Vector3.Cross
                                        (
                                            _motor.CharacterUp,
                                            _motor.GroundingStatus.GroundNormal
                                        )
                                    ).normalized;

                                    // project movement force onto obstruction plane
                                    movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);
                                }
                            }

                            currentVelocity += movementForce;
                        }

                        //Gravity
                        var effectiveGravity = _gravity;
                        var verticalSpeed = Vector3.Dot(currentVelocity, _motor.CharacterUp);
                        if (_requestedInput.SustainJump && verticalSpeed > 0f)
                            effectiveGravity *= _jumpSustainGravity;

                        currentVelocity += _motor.CharacterUp * effectiveGravity * deltaTime;

                        if (_grapplingSwing.IsSwing)
                        {
                            _grapplingSwing.SwingMovement(ref currentVelocity, deltaTime);
                        }
                    }
                }
                //Jump
                if (_requestedInput.Jump)
                {
                    var grounded = _motor.GroundingStatus.IsStableOnGround;
                    var canCoyoteJump = _timeSinceUngrounded < _coyoteTime && !_ungroundedDueToJump;

                    if (_grapplingSwing.IsSwing && !grounded)
                    {
                        _remainJumpCount = 1;
                        _requestedInput.Jump = false;     //Unset jump request
                        _requestedInput.Crouch = false;   // and request the character uncrouch
                        _requestedInput.CrouchInAir = false;
                        _grapplingSwing.StopGrapplingSwing();
                    }

                    else if (_wallRunning.IsWallRunning)
                    {
                        _remainJumpCount--;
                        _requestedInput.Jump = false;     //Unset jump request
                        _requestedInput.Crouch = false;   // and request the character uncrouch
                        _requestedInput.CrouchInAir = false;
                        _wallRunning.WallJump(ref currentVelocity);
                    }

                    else if (_wallClimb.IsClimbing)
                    {
                        _remainJumpCount--;
                        _requestedInput.Jump = false;     //Unset jump request
                        _requestedInput.Crouch = false;   // and request the character uncrouch
                        _requestedInput.CrouchInAir = false;
                        _wallClimb.ClimbJump(ref currentVelocity);
                    }

                    else if (grounded || canCoyoteJump || _remainJumpCount > 0)
                    {
                        _remainJumpCount--;
                        _requestedInput.Jump = false;     //Unset jump request
                        _requestedInput.Crouch = false;   // and request the character uncrouch
                        _requestedInput.CrouchInAir = false;

                        // Unstick Player from the ground
                        _motor.ForceUnground(time: 0f);
                        _ungroundedDueToJump = true;

                        // Set Minimum Vertical Speed to the Jump Speed
                        var currentVerticalSpeed = Vector3.Dot(currentVelocity, _motor.CharacterUp);
                        var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _jumpSpeed);

                        // Add the difference in current and target vertical speed to the character's velocity
                        currentVelocity += _motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
                    }
                    else
                    {
                        _timeSinceJumpRequest += deltaTime;

                        //Defer the jump request until coyote time has passed
                        var canJumpLater = _timeSinceJumpRequest < _coyoteTime;
                        _requestedInput.Jump = canJumpLater;
                    }
                }
                break;
            case EState.Dash:
                if (_timeSinceDash > 0)
                {
                    currentVelocity = _currentDashVelocity;
                }
                break;
        }
    }
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        /*
         * Update the character's rotation to same direction as the
         * requested roation <- camera rotation
         * 
         * Dont want character to pitch Up or Down
         * -> Direction the character looks always flattened
         * 
         * Projecting Vector pointing in same direction
         * player is looking onto a flat ground plane
         */

        var forward = Vector3.ProjectOnPlane
        (
            _requestedInput.Rotation * Vector3.forward, _motor.CharacterUp
        );

        if (forward != Vector3.zero)
            currentRotation = Quaternion.LookRotation(forward, _motor.CharacterUp);
    }


    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = _currentState;

        // Crocuh
        if (_requestedInput.Crouch && _currentState.Stance is EStance.Stand)
        {
            _currentState.Stance = EStance.Crouch;
            _motor.SetCapsuleDimensions
            (
                radius: _motor.Capsule.radius,
                height: _crouchHeight,
                yOffset: _crouchHeight * 0.5f
            );
        }
    }
    public void PostGroundingUpdate(float deltaTime)
    {
        if (!_motor.GroundingStatus.IsStableOnGround && _currentState.Stance is EStance.Slide)
            _currentState.Stance = EStance.Crouch;
    }
    public void AfterCharacterUpdate(float deltaTime)
    {
        switch (_state)
        {
            case EState.Default:
                // Uncrouch
                if (!_requestedInput.Crouch && _currentState.Stance != EStance.Stand)
                {
                    // Tentatively stand up the character capsule
                    _motor.SetCapsuleDimensions
                    (
                        radius: _motor.Capsule.radius,
                        height: _standHeight,
                        yOffset: _standHeight * 0.5f
                    );

                    // Check if the capsule overlap any collider before
                    // allow character to standup
                    var pos = _motor.TransientPosition;
                    var rot = _motor.TransientRotation;
                    var mask = _motor.CollidableLayers;

                    if (_motor.CharacterOverlap(pos, rot, _uncrouchOverlapResults, mask, QueryTriggerInteraction.Ignore) > 0)
                    {
                        // Crouch again
                        _requestedInput.Crouch = true;
                        _motor.SetCapsuleDimensions
                        (
                            radius: _motor.Capsule.radius,
                            height: _crouchHeight,
                            yOffset: _crouchHeight * 0.5f
                        );
                    }
                    else
                    {
                        _currentState.Stance = EStance.Stand;
                    }
                }

                _dashCooldownTimer -= deltaTime;
                break;
            case EState.Dash:
                if (_timeSinceDash > 0f)
                    _timeSinceDash -= deltaTime;
                else
                    ChangeState(EState.Default);
                break;
        }

        //Update state to reflect relevant motor properties
        _currentState.Grounded = _motor.GroundingStatus.IsStableOnGround;
        _currentState.Velocity = _motor.Velocity;
        // update _lastState to store the Character state snapshot taken at
        // the beginning of this character update;
        _lastState = _tempState;
    }


    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        if (_grapplingSwing.IsSwing)
        {
            _grapplingSwing.StopGrapplingSwing();
        }
        if (_currentState.Stance == EStance.Stand || _currentState.Stance == EStance.Crouch)
        {
            if (_motor.Velocity.magnitude < 40f)
                _playerCamera.DoFov(80f);
        }
    }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        if (hitStabilityReport.IsStable)
        { ResetJumpCount(); }

        if (_grapplingSwing.IsSwing)
        {
            _grapplingSwing.StopGrapplingSwing();
        }
    }
    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public Transform GetCameraTarget()
    {
        return _cameraTarget;
    }

    public CharacterState GetState() => _currentState;
    public CharacterState GetLastState() => _lastState;

    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        _motor.SetPosition(position);
        if (killVelocity)
        {
            _motor.BaseVelocity = Vector3.zero;
        }
    }

    private void ResetJumpCount()
    {
        _remainJumpCount = _maxJumpCount;
    }

    /// <summary>
    /// Check Is Player is not StableGround
    /// </summary>
    /// <returns></returns>
    public bool AboveGound()
    {
        return !_motor.GroundingStatus.IsStableOnGround;
    }

    #region Character State
    private void ChangeState(EState newState)
    {
        EState tmpInitialState = _state;
        OnStateExit(tmpInitialState, newState);
        _state = newState;
        OnStateEnter(newState, tmpInitialState);
    }

    private void OnStateEnter(EState state, EState fromState)
    {
        switch (state)
        {
            case EState.Default:
                break;
            case EState.Dash:
                _playerCamera.DoFov(100f);
                _currentDashVelocity = _cameraTransform.forward * _dashSpeed;
                _currentDashVelocity =
                    _currentDashVelocity.magnitude > _motor.Velocity.magnitude
                    ? _currentDashVelocity : _cameraTransform.forward * _motor.Velocity.magnitude;
                _timeSinceDash = _dashTime;
                break;

        }
    }

    private void OnStateExit(EState state, EState toState)
    {
        switch (state)
        {
            case EState.Default:
                break;
            case EState.Dash:
                _playerCamera.DoFov(80f);
                _dashCooldownTimer = _dashCooldownTime;
                break;
        }
    }
    #endregion Character State
}