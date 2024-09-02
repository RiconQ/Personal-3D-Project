using KinematicCharacterController;
using System;
using TMPro;
using UnityEngine;

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
    public bool Jump;
    public bool JumpSustain;
    public ECrouchInput Crouch;
}

public struct RequestedInput
{
    public Quaternion Rotation;
    public Vector3 Movement;
    public bool Jump;
    public bool SustainJump;
    public bool Crouch;
    public bool CrouchInAir;
}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    #region Variables

    [Header("Assignables")]
    [SerializeField] private KinematicCharacterMotor _motor;
    [SerializeField] private Transform _root;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private PlayerCamera _playerCamera;

    [Space]
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 20f;
    [SerializeField] private float _crouchSpeed = 7f;

    [Space]
    [SerializeField] private float _walkResponse = 25f;
    [SerializeField] private float _crouchResponse = 20f;

    [Header("Wall Running, Wall Jumping")]
    [SerializeField] private LayerMask _whatIsWall;
    [SerializeField] private LayerMask _whatIsGround;
    [SerializeField] private float _cameraZTilt = 7.5f;
    [Space]
    [SerializeField] private float _wallRunSpeed = 30f;
    [SerializeField] private float _wallJumpForce = 10f;
    [SerializeField] private float _wallSideJumpForce = 10f;
    [Space]
    [SerializeField] private float _exitWallTime = 0.2f;
    [Space]
    [SerializeField] private bool _useWallCounterForce = true;
    [SerializeField] private float _wallCounterForce = 10f;

    [Header("Wall Detection")]
    [SerializeField] private float _wallCheckDistance = 3f;
    //[SerializeField] private float _minJumpHeight = 0.5f;

    [Header("Jump")]
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

    private CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState;

    // Input
    private RequestedInput _requestedInput;
    private Vector2 _moveInput;

    #region Code Variables
    // Jump
    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequest;
    private bool _ungroundedDueToJump;

    #region Wall Run
    // Wall Detection
    private RaycastHit _rightWallHit;
    private RaycastHit _leftWallHit;
    private bool _isWallRight;
    private bool _isWallLeft;

    // Wall Run
    private bool _isWallRunning;
    private bool _isExitingWall;
    private float _exitWallTimer;
    #endregion

    private Collider[] _uncrouchOverlapResults;
    #endregion

    [Header("Debug")]
    public TMP_Text debugText;

    #endregion

    public void Initialize()
    {
        _state.Stance = EStance.Stand;
        _lastState = _state;
        _uncrouchOverlapResults = new Collider[8];

        _motor.CharacterController = this;

        ResetVariables();
    }

    private void ResetVariables()
    {
        //reset wallRun var
        _isWallLeft = false;
        _isWallRight = false;
        _isWallRunning = false;
        _isExitingWall = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
            debugText.gameObject.SetActive(!debugText.gameObject.activeSelf);
        //Debug
        string debugLog =
            $"Stance : {_state.Stance.ToString()}\n" +
            $"Grounded : {_state.Grounded}\n" +
            $"isWallRight : {_isWallRight}\n" +
            $"isWallLeft : {_isWallLeft}\n" +
            $"";

        debugText.text = $"{debugLog}";




        CheckForWall();
        WallRunState();

    }

    public void UpdateInput(CharacterInput input, Vector2 moveInput)
    {
        _moveInput = moveInput;

        _requestedInput.Rotation = input.Rotation;
        // Take Vector2 Input -> Create 3D Movement Vector on XZ Plane
        _requestedInput.Movement = new Vector3(input.Move.x, 0f, input.Move.y);
        // Clamp to 1 length
        _requestedInput.Movement = Vector3.ClampMagnitude(_requestedInput.Movement, 1f);
        // Orient the input so it's relative to the direction the player is facing
        _requestedInput.Movement = input.Rotation * _requestedInput.Movement;

        //Debug.Log(_motor.Velocity);

        var wasRequestingJump = _requestedInput.Jump;
        _requestedInput.Jump = _requestedInput.Jump || input.Jump;
        if (_requestedInput.Jump && !wasRequestingJump)
            _timeSinceJumpRequest = 0f;

        _requestedInput.SustainJump = input.JumpSustain;

        var wasRequestingCrouch = _requestedInput.Crouch;
        _requestedInput.Crouch = input.Crouch switch
        {
            ECrouchInput.Toggle => !_requestedInput.Crouch,
            ECrouchInput.None => _requestedInput.Crouch,
            _ => _requestedInput.Crouch
        };

        if (_requestedInput.Crouch && !wasRequestingCrouch)
            _requestedInput.CrouchInAir = !_state.Grounded;
        else if (!_requestedInput.Crouch && wasRequestingCrouch)
            _requestedInput.CrouchInAir = false;
    }

    public void UpdateBody(float deltaTime)
    {
        var currentHeight = _motor.Capsule.height;
        var normalizedHeight = currentHeight / _standHeight;

        var cameraTargetHeight = currentHeight *
        (
            _state.Stance is EStance.Stand ? _standCameraTargetHeight : _crouchCameraTargetHeight
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
        _state.Acceleration = Vector3.zero;

        // If player on Ground
        if (_motor.GroundingStatus.IsStableOnGround)
        {
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
                var crouching = _state.Stance is EStance.Crouch;
                var wasStanding = _lastState.Stance is EStance.Stand;
                var wasInAir = !_lastState.Grounded;
                if (moving && crouching && (wasStanding || wasInAir))
                {
                    _state.Stance = EStance.Slide;

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
            if (_state.Stance is EStance.Stand || _state.Stance is EStance.Crouch)
            {
                // Calculate the speed and responsiveness of movement based
                //on the character's stance
                var speed = _state.Stance is EStance.Stand ? _walkSpeed : _crouchSpeed;
                var response = _state.Stance is EStance.Stand ? _walkResponse : _crouchResponse;

                // Smooth move along the ground in that direction
                var targetVelocity = groundedMovement * speed;
                var moveVelocity = Vector3.Lerp
                (
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-response * deltaTime)
                );
                _state.Acceleration = moveVelocity - currentVelocity;

                currentVelocity = moveVelocity;
            }
            // Continue sliding
            else
            {
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

                    _state.Acceleration = (steerVelocity - currentVelocity) / deltaTime;

                    currentVelocity = steerVelocity;
                }

                //Stop
                if (currentVelocity.magnitude < _slideEndSpeed)
                    _state.Stance = EStance.Crouch;
            }
        }
        // character is in air
        else
        {
            if (_isWallRunning)
            {
                WallRunningMovement(ref currentVelocity, deltaTime);
            }

            else
            {
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
            }
        }

        //Jump
        if (_requestedInput.Jump)
        {
            var grounded = _motor.GroundingStatus.IsStableOnGround;
            var canCoyoteJump = _timeSinceUngrounded < _coyoteTime && !_ungroundedDueToJump;

            if (_isWallRunning)
            {
                _requestedInput.Jump = false;     //Unset jump request
                _requestedInput.Crouch = false;   // and request the character uncrouch
                _requestedInput.CrouchInAir = false;
                WallJump(ref currentVelocity, deltaTime);
            }

            if (grounded || canCoyoteJump)
            {
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
        _tempState = _state;

        // Crocuh
        if (_requestedInput.Crouch && _state.Stance is EStance.Stand)
        {
            _state.Stance = EStance.Crouch;
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
        if (!_motor.GroundingStatus.IsStableOnGround && _state.Stance is EStance.Slide)
            _state.Stance = EStance.Crouch;
    }
    public void AfterCharacterUpdate(float deltaTime)
    {
        // Uncrouch
        if (!_requestedInput.Crouch && _state.Stance != EStance.Stand)
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
                _state.Stance = EStance.Stand;
            }
        }

        //Update state to reflect relevant motor properties
        _state.Grounded = _motor.GroundingStatus.IsStableOnGround;
        _state.Velocity = _motor.Velocity;
        // update _lastState to store the Character state snapshot taken at
        // the beginning of this character update;
        _lastState = _tempState;
    }


    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
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

    public CharacterState GetState() => _state;
    public CharacterState GetLastState() => _lastState;

    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        _motor.SetPosition(position);
        if (killVelocity)
        {
            _motor.BaseVelocity = Vector3.zero;
        }
    }

    #region Wall Run

    private void WallRunState()
    {
        if ((_isWallLeft || _isWallRight) && _moveInput.y > 0 && AboveGound() && !_isExitingWall)
        {
            if (!_isWallRunning)
            {
                StartWallRun();
            }
        }
        else if (_isExitingWall)
        {
            if (_isWallRunning)
                StopWallRun();
        
            if (_exitWallTimer > 0)
                _exitWallTimer -= Time.deltaTime;
        
            if (_exitWallTimer <= 0)
                _isExitingWall = false;
        }
        else
        {
            if (_isWallRunning)
                StopWallRun();
        }
    }

    /// <summary>
    /// Check Wall is left or right
    /// </summary>
    private void CheckForWall()
    {

        //Debug.Log("CheckWall");
        _isWallRight = Physics.Raycast(transform.position, transform.right, out _rightWallHit, _wallCheckDistance, _whatIsWall);
        _isWallLeft = Physics.Raycast(transform.position, -transform.right, out _leftWallHit, _wallCheckDistance, _whatIsWall);

        /*
        //var crossProduct = Vector3.Cross(_motor.CharacterForward, hitNormal);
        
        //if (crossProduct.y > 0)
        //{
        //    _isWallRight = false;
        //    _isWallLeft = true;
        //}
        //else if (crossProduct.y < 0)
        //{
        //    _isWallRight = true;
        //    _isWallLeft = false;
        //}
        */
    }

    /// <summary>
    /// Check Is Player is not StableGround
    /// </summary>
    /// <returns></returns>
    private bool AboveGound()
    {
        return !_motor.GroundingStatus.IsStableOnGround;
    }

    private void StartWallRun()
    {
        //Debug.Log("Start WallRun");
        _isWallRunning = true;

        // Change Fov
        _playerCamera.DoFov(90f);
        // Do Tilt
        if (_isWallRight) _playerCamera.DoTilt(_cameraZTilt);
        if (_isWallLeft) _playerCamera.DoTilt(-_cameraZTilt);
    }

    private void WallRunningMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        var wallNoramal = _isWallRight ? _rightWallHit.normal : _leftWallHit.normal;
        var wallForward = Vector3.Cross(wallNoramal, transform.up);

        // Modify Wall Forward to correct direction
        if ((transform.forward - wallForward).magnitude > (transform.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        currentVelocity = wallForward * _wallRunSpeed;

        // Add Velocity for Stick on wall
        if (_useWallCounterForce)
        {
            if (!(_isWallLeft && _moveInput.x > 0) && !(_isWallRight && _moveInput.x < 0))
                currentVelocity += -wallNoramal * _wallCounterForce;
        }
    }

    private void WallJump(ref Vector3 currentVelocity, float deltaTime)
    {
        Debug.Log("Wall Jump");
        _isExitingWall = true;
        _exitWallTimer = _exitWallTime;

        Vector3 wallNormal = _isWallRight ? _rightWallHit.normal : _leftWallHit.normal;
        Vector3 forceToApply = _motor.CharacterUp * _wallJumpForce + wallNormal * _wallSideJumpForce;

        //test
        // Unstick Player from the ground
        _motor.ForceUnground(time: 0f);
        _ungroundedDueToJump = true;

        // Set Minimum Vertical Speed to the Jump Speed
        var currentVerticalSpeed = Vector3.Dot(currentVelocity, _motor.CharacterUp);
        var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _wallJumpForce);

        // Add the difference in current and target vertical speed to the character's velocity
        currentVelocity += forceToApply * (targetVerticalSpeed - currentVerticalSpeed);
    }
    private void StopWallRun()
    {
        _isWallRunning = false;

        //Change Fov
        _playerCamera.DoFov(80f);
        //reset Tilt
        _playerCamera.DoTilt(0f);
    }
    #endregion
}
