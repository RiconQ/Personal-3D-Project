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

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [Header("Assignables")]
    [SerializeField] private KinematicCharacterMotor _motor;
    [SerializeField] private Transform _root;
    [SerializeField] private Transform _cameraTarget;

    [Space]
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 20f;
    [SerializeField] private float _crouchSpeed = 7f;

    [Space]
    [SerializeField] private float _walkResponse = 25f;
    [SerializeField] private float _crouchResponse = 20f;

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

    // Requested Input
    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedSustainJump;
    private bool _requestedCrouch;
    private bool _requestedCrouchInAir;

    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequest;
    private bool _ungroundedDueToJump;

    private Collider[] _uncrouchOverlapResults;


    [Header("Debug")]
    public TMP_Text debugText;

    public void Initialize()
    {
        _state.Stance = EStance.Stand;
        _lastState = _state;
        _uncrouchOverlapResults = new Collider[8];

        _motor.CharacterController = this;
    }

    private void Update()
    {
        string playerStance = $"Stance : {_state.Stance.ToString()}";
        string isGround = $"Grounded : {_state.Grounded}";
        debugText.text = $"{playerStance}\n{isGround}";
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;
        // Take Vector2 Input -> Create 3D Movement Vector on XZ Plane
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        // Clamp to 1 length
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        // Orient the input so it's relative to the direction the player is facing
        _requestedMovement = input.Rotation * _requestedMovement;

        var wasRequestingJump = _requestedJump;
        _requestedJump = _requestedJump || input.Jump;
        if (_requestedJump && !wasRequestingJump)
            _timeSinceJumpRequest = 0f;

        _requestedSustainJump = input.JumpSustain;

        var wasRequestingCrouch = _requestedCrouch;
        _requestedCrouch = input.Crouch switch
        {
            ECrouchInput.Toggle => !_requestedCrouch,
            ECrouchInput.None => _requestedCrouch,
            _ => _requestedCrouch
        };

        if (_requestedCrouch && !wasRequestingCrouch)
            _requestedCrouchInAir = !_state.Grounded;
        else if (!_requestedCrouch && wasRequestingCrouch)
            _requestedCrouchInAir = false;
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
                direction: _requestedMovement,
                surfaceNormal: _motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

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
                    if (!_lastState.Grounded && !_requestedCrouchInAir)
                    {
                        effectiveSlideStartSpeed = 0f;
                        _requestedCrouchInAir = false;
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

                    _state.Acceleration = steerVelocity - currentVelocity;

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
            _timeSinceUngrounded += deltaTime;

            // Air Move
            if (_requestedMovement.sqrMagnitude > 0f)
            {
                // Requested movement projected onto movement plane (magnitude preserved)
                var planarMovement = Vector3.ProjectOnPlane
                (
                    vector: _requestedMovement,
                    planeNormal: _motor.CharacterUp
                ) * _requestedMovement.magnitude;

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
            if (_requestedSustainJump && verticalSpeed > 0f)
                effectiveGravity *= _jumpSustainGravity;

            currentVelocity += _motor.CharacterUp * effectiveGravity * deltaTime;
        }

        //Jump
        if (_requestedJump)
        {
            var grounded = _motor.GroundingStatus.IsStableOnGround;
            var canCoyoteJump = _timeSinceUngrounded < _coyoteTime && !_ungroundedDueToJump;

            if (grounded || canCoyoteJump)
            {
                _requestedJump = false;     //Unset jump request
                _requestedCrouch = false;   // and request the character uncrouch
                _requestedCrouchInAir = false;

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
                _requestedJump = canJumpLater;
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
            _requestedRotation * Vector3.forward, _motor.CharacterUp
        );

        if (forward != Vector3.zero)
            currentRotation = Quaternion.LookRotation(forward, _motor.CharacterUp);
    }


    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = _state;

        // Crocuh
        if (_requestedCrouch && _state.Stance is EStance.Stand)
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
        if (!_requestedCrouch && _state.Stance != EStance.Stand)
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
                _requestedCrouch = true;
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
}
