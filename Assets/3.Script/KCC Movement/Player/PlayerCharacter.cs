using KinematicCharacterController;
using System;
using System.Collections.Generic;
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
    public ECrouchInput Crouch;
    public bool LeftMouse;
    public bool LeftMouseReleased;
    public bool RightMouse;
    public bool RightMouseReleased;
}

public struct RequestedInput
{
    public Quaternion Rotation;
    public Vector3 Movement;
    public bool Jump;
    public bool Crouch;
    public bool CrouchInAir;
    public bool LeftMouse;
    public bool LeftMouseReleased;
    public bool RightMouse;
    public bool RightMouseReleased;
}

public class PlayerState
{
    public bool isExitingWall = false;
    public bool isLedgeClimbing = false;
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
    [SerializeField] private Player _player;

    // Movement Reference
    private Jump _jump;
    private Sliding _sliding;
    private LedgeClimb _ledgeClimb;
    private ObjectThrow _objectThrow;

    [Space]
    [Header("Movement")]
    [SerializeField] private float _gravity = -90f;
    public float Gravity => _gravity;
    [SerializeField] private float _walkSpeed = 20f;
    [SerializeField] private float _crouchSpeed = 7f;

    [Space]
    [SerializeField] private float _walkResponse = 25f;
    [SerializeField] private float _crouchResponse = 20f;
    [SerializeField] private float _exitWallTime = 0.2f;
    public float ExitWallTime => _exitWallTime;

    [Header("Weapon")]
    [SerializeField] private K_SwordController _sword;

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

    public CharacterState currentState;
    public CharacterState lastState;
    private CharacterState _tempState;

    // Input
    public RequestedInput requestedInput;
    private Vector2 _moveInput;
    public Vector2 MoveInput => _moveInput;

    #region Code Variables
    [HideInInspector] public float exitWallTimer;

    private Collider[] _uncrouchOverlapResults;

    [HideInInspector] public List<Collider> ignoredCollider = new List<Collider>();

    #endregion Code Variables

    [Header("Debug")]
    public TMP_Text debugText;

    #endregion Variables

    public KinematicCharacterMotor Motor => _motor;

    public void Initialize()
    {
        currentState.Stance = EStance.Stand;
        lastState = currentState;
        _uncrouchOverlapResults = new Collider[8];

        _motor.CharacterController = this;

        GetMovementReference();
    }

    private void GetMovementReference()
    {
        _jump = GetComponent<Jump>();
        _sliding = GetComponent<Sliding>();
        _ledgeClimb = GetComponent<LedgeClimb>();
        _objectThrow = GetComponent<ObjectThrow>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
            debugText.gameObject.SetActive(!debugText.gameObject.activeSelf);
        //Debug
        string debugLog =
            $"Stance : {currentState.Stance.ToString()}\n" +
            $"Grounded : {currentState.Grounded}\n" +
            $"Velocity : {_motor.Velocity.magnitude}\n" +
            $"FOV : {Camera.main.fieldOfView}\n";

        debugText.text = $"{debugLog}";
    }

    private void LateUpdate()
    {
        Debug.DrawRay(transform.position, _motor.Velocity.normalized * 2, Color.yellow);
    }

    public void UpdateInput(CharacterInput input, Vector2 moveInput)
    {
        #region Move Vector2
        _moveInput = moveInput;
        #endregion

        #region Mouse
        requestedInput.Rotation = input.Rotation;
        #endregion

        #region Movement
        // Take Vector2 Input -> Create 3D Movement Vector on XZ Plane
        requestedInput.Movement = new Vector3(input.Move.x, 0f, input.Move.y);
        // Clamp to 1 length
        requestedInput.Movement = Vector3.ClampMagnitude(requestedInput.Movement, 1f);
        // Orient the input so it's relative to the direction the player is facing
        requestedInput.Movement = input.Rotation * requestedInput.Movement;
        #endregion

        #region Jump
        var wasRequestingJump = requestedInput.Jump;
        requestedInput.Jump = requestedInput.Jump || input.Jump;
        if (requestedInput.Jump && !wasRequestingJump)
        {
            _jump.timeSinceJumpRequest = 0f;
        }

        #endregion

        #region Crouch
        var wasRequestingCrouch = requestedInput.Crouch;
        requestedInput.Crouch = input.Crouch switch
        {
            ECrouchInput.Toggle => !requestedInput.Crouch,
            ECrouchInput.None => requestedInput.Crouch,
            _ => requestedInput.Crouch
        };

        if (requestedInput.Crouch && !wasRequestingCrouch)
        {
            requestedInput.CrouchInAir = !currentState.Grounded;
        }
        else if (!requestedInput.Crouch && wasRequestingCrouch)
        {
            requestedInput.CrouchInAir = false;
        }
        #endregion

        #region LeftMouse
        requestedInput.LeftMouse = input.LeftMouse;
        requestedInput.LeftMouseReleased = input.LeftMouseReleased;
        if (!_player.WeaponHolder.HasWaepon())
        {
            if (requestedInput.LeftMouse && _objectThrow.ObjectToThrow == null)
            {
                _objectThrow.PickUpObject();
            }
            if (requestedInput.LeftMouseReleased && _objectThrow.ReadyToThrow)
            {
                //Debug.Log("Throw");
                _objectThrow.Throw();
            }
        }
        #endregion

        /*
        #region RightMouse
        requestedInput.RightMouse = input.RightMouse;
        requestedInput.RightMouseReleased = input.RightMouseReleased;
        if (!_player.WeaponHolder.HasWaepon())
        {
            if (!_objectThrow.ReadyToThrow)
            {
                if (requestedInput.RightMouse && !_grappling.IsGrappling)
                {
                    _grappling.ShootGrapple();
                }
                else if (requestedInput.RightMouse && _grappling.HasGrapplePoint)
                {
                    //Jump to GrapplePoint
                    _grappling.StopGrappling();
                }
                else if (requestedInput.RightMouseReleased && _grappling.HasGrapplePoint)
                {
                    _grappling.ExecuteGrapple();
                }
                else if (requestedInput.RightMouseReleased && _grappling.CurrentGrabedObject != null)
                {
                    Debug.Log("Pull");
                    _grappling.PullObject();
                }
            }
        }
        #endregion
        */
    }

    public void UpdateBody(float deltaTime)
    {
        var currentHeight = _motor.Capsule.height;
        var normalizedHeight = currentHeight / _standHeight;

        var cameraTargetHeight = currentHeight *
        (
            currentState.Stance is EStance.Stand ? _standCameraTargetHeight : _crouchCameraTargetHeight
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
        currentState.Acceleration = Vector3.zero;

        if (_ledgeClimb.IsLedgeClimbing == true)
        {
            return;
        }

        /*
        if (_grappling.IsGrappleExecuting == true)
        {
            _grappling.JumpToPosition(ref currentVelocity);
            return;
        }
        */

        // If player on Ground
        if (_motor.GroundingStatus.IsStableOnGround)
        {
            _jump.timeSinceUngrounded = 0f;
            _jump.ungroundedDueToJump = false;

            //snap the request movement direction to the angle of surface
            // -> character is walking on Ground
            var groundedMovement = _motor.GetDirectionTangentToSurface
            (
                direction: requestedInput.Movement,
                surfaceNormal: _motor.GroundingStatus.GroundNormal
            ) * requestedInput.Movement.magnitude;

            // Start Sliding
            _sliding.StartSliding(ref currentVelocity, groundedMovement);

            // Move
            if (currentState.Stance is EStance.Stand || currentState.Stance is EStance.Crouch)
            {
                // Calculate the speed and responsiveness of movement based
                //on the character's stance
                var speed = currentState.Stance is EStance.Stand ? _walkSpeed : _crouchSpeed;
                var response = currentState.Stance is EStance.Stand ? _walkResponse : _crouchResponse;

                // Smooth move along the ground in that direction
                var targetVelocity = groundedMovement * speed;
                var moveVelocity = Vector3.Lerp
                (
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-response * deltaTime)
                );
                currentState.Acceleration = moveVelocity - currentVelocity;

                currentVelocity = moveVelocity;
            }
            // Continue sliding
            else
            {
                _playerCamera.DoFov(110f);
                _sliding.ContinueSliding(ref currentVelocity, deltaTime, groundedMovement);
            }
        }
        // character is in air
        else
        {
            if (_motor.Velocity.magnitude > 50)
            {
                _playerCamera.DoFov(110f);
            }
            else
            {
                _playerCamera.DoFov(100f);
            }
            _jump.timeSinceUngrounded += deltaTime;

            // Air Move
            if (requestedInput.Movement.sqrMagnitude > 0f)
            {
                // Requested movement projected onto movement plane (magnitude preserved)
                var planarMovement = Vector3.ProjectOnPlane
                (
                    vector: requestedInput.Movement,
                    planeNormal: _motor.CharacterUp
                ) * requestedInput.Movement.magnitude;

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

            currentVelocity += _motor.CharacterUp * effectiveGravity * deltaTime;

        }
        //Jump
        if (requestedInput.Jump)
        {
            var grounded = _motor.GroundingStatus.IsStableOnGround;
            var canCoyoteJump = _jump.timeSinceUngrounded < _jump.CoyoteTime && !_jump.ungroundedDueToJump;

            if (grounded || canCoyoteJump || currentState.Stance == EStance.Slide)
            {
                requestedInput.Jump = false;     //Unset jump request
                requestedInput.Crouch = false;   // and request the character uncrouch
                requestedInput.CrouchInAir = false;

                // Unstick Player from the ground
                _motor.ForceUnground(time: 0f);
                _jump.ungroundedDueToJump = true;

                if (currentState.Stance == EStance.Slide)
                {
                    _sliding.SlidingJump(ref currentVelocity);
                }
                else
                {
                    _jump.JumpMovement(ref currentVelocity);
                }
            }
            else if (!grounded && _ledgeClimb.IsInLedge && !_ledgeClimb.IsLedgeClimbing)
            {
                requestedInput.Jump = false;     //Unset jump request
                requestedInput.Crouch = false;   // and request the character uncrouch
                requestedInput.CrouchInAir = false;

                _ledgeClimb.StartLedgeClimb();
            }
            else
            {
                _jump.timeSinceJumpRequest += deltaTime;

                //Defer the jump request until coyote time has passed
                var canJumpLater = _jump.timeSinceJumpRequest < _jump.CoyoteTime;
                requestedInput.Jump = canJumpLater;
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
            requestedInput.Rotation * Vector3.forward, _motor.CharacterUp
        );

        if (forward != Vector3.zero)
            currentRotation = Quaternion.LookRotation(forward, _motor.CharacterUp);
    }


    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = currentState;

        // Crocuh
        if (requestedInput.Crouch && currentState.Stance is EStance.Stand)
        {
            currentState.Stance = EStance.Crouch;
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
        if (!_motor.GroundingStatus.IsStableOnGround && currentState.Stance is EStance.Slide)
            currentState.Stance = EStance.Crouch;
    }
    public void AfterCharacterUpdate(float deltaTime)
    {
        // Uncrouch
        if (!requestedInput.Crouch && currentState.Stance != EStance.Stand)
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
                requestedInput.Crouch = true;
                _motor.SetCapsuleDimensions
                (
                    radius: _motor.Capsule.radius,
                    height: _crouchHeight,
                    yOffset: _crouchHeight * 0.5f
                );
            }
            else
            {
                currentState.Stance = EStance.Stand;
            }
        }

        //Update state to reflect relevant motor properties
        currentState.Grounded = _motor.GroundingStatus.IsStableOnGround;
        currentState.Velocity = _motor.Velocity;
        // update _lastState to store the Character state snapshot taken at
        // the beginning of this character update;
        lastState = _tempState;
    }


    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        if (currentState.Stance == EStance.Stand || currentState.Stance == EStance.Crouch)
        {
            if (_motor.Velocity.magnitude < 40f)
                _playerCamera.DoFov(100f);
        }

        
        if (_sword.gameObject.activeSelf && _sword.IsAirStrike)
        {
            _sword.AirStrike();
        }
    }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        /*
        if (_grappling.IsGrappling)
        {
            //If Hit Enemy -> Enemy Jump
        }
        */
    }
    public bool IsColliderValidForCollisions(Collider coll)
    {
        if (ignoredCollider.Contains(coll))
        {
            return false;
        }
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

    public CharacterState GetState() => currentState;
    public CharacterState GetLastState() => lastState;

    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        _motor.SetPosition(position);
        if (killVelocity)
        {
            _motor.BaseVelocity = Vector3.zero;
        }
    }
}