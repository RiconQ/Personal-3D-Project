using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum EMovementState
{
    Freeze,
    Walking,
    Sprinting,
    Crouching,
    Climbing,
    WallRunning,
    Sliding,
    Air
}

public class Legacy_PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float _moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallRunSpeed;
    public float climbSpeed;

    private float _desiredMoveSpeed;
    private float _lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump;

    [Header("Crouching")]
    public float crouchspeed;
    public float crouchYScale;
    private float _startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit _slopeHit;
    private bool _exitingSlope;

    public Transform orientation;

    private float _horizontalInput;
    private float _verticalInput;

    private Vector3 _moveDirection;
    private Rigidbody _rb;

    public EMovementState state;

    public bool isSliding;
    public bool wallRunning;
    public bool isClimbing;
    public bool isFreeze;
    public bool activeGrapple = false;

    private bool enableMovementOnNextTouch;


    [Header("Reference")]
    public Legacy_Climbing _climbingScript;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        readyToJump = true;

        _startYScale = transform.localScale.y;
    }

    private void Update()
    {
        //Ground Check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        //handle drag
        if (grounded && !activeGrapple)
            _rb.drag = groundDrag;
        else
            _rb.drag = 0;

    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        //when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Start Crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // Stop Crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, _startYScale, transform.localScale.z);
        }
    }

    private void MovePlayer()
    {
        if (activeGrapple) return;

        if (_climbingScript.exitingWall) return;
        _moveDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;

        // on slope
        if (OnSlope() && !_exitingSlope)
        {
            _rb.AddForce(GetSlopeMoveDirection(_moveDirection) * _moveSpeed * 20f, ForceMode.Force);

            if (_rb.velocity.y > 0)
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        //on ground
        if (grounded)
        {
            //Debug.Log("Grounded");
            //Debug.Log(_moveDirection.normalized);
            _rb.AddForce(_moveDirection.normalized * _moveSpeed * 10f, ForceMode.Force);
        }
        // in air
        else if (!grounded)
            _rb.AddForce(_moveDirection.normalized * _moveSpeed * 10f * airMultiplier, ForceMode.Force);

        //turn gravity off while on slope
        if (!wallRunning)
            _rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (activeGrapple) return;

        if (OnSlope() && !_exitingSlope)
        {
            if (_rb.velocity.magnitude > _moveSpeed)
                _rb.velocity = _rb.velocity.normalized * _moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

            if (flatVel.magnitude > _moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * _moveSpeed;
                _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        _exitingSlope = true;
        // reset y velocity
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        _exitingSlope = false;
    }

    private void StateHandler()
    {
        if (isFreeze)
        {
            state = EMovementState.Freeze;
            _moveSpeed = 0f;
            _rb.velocity = Vector3.zero;
        }

        if (isClimbing)
        {
            state = EMovementState.Climbing;
            _desiredMoveSpeed = climbSpeed;
        }

        else if (wallRunning)
        {
            state = EMovementState.WallRunning;
            _desiredMoveSpeed = wallRunSpeed;
        }

        // sliding
        else if (isSliding)
        {
            state = EMovementState.Sliding;

            if (OnSlope() && _rb.velocity.y < 0.1f)
            {
                _desiredMoveSpeed = slideSpeed;
            }

            else
            {
                _desiredMoveSpeed = sprintSpeed;
            }
        }

        // Crouching
        else if (Input.GetKey(crouchKey))
        {
            state = EMovementState.Crouching;
            _desiredMoveSpeed = crouchspeed;
        }

        // Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = EMovementState.Sprinting;
            _desiredMoveSpeed = sprintSpeed;
        }
        // Walking
        else if (grounded)
        {
            state = EMovementState.Walking;
            _desiredMoveSpeed = walkSpeed;
        }
        // Air
        else
        {
            state = EMovementState.Air;
        }

        if (Mathf.Abs(_desiredMoveSpeed - _lastDesiredMoveSpeed) > 4f && _moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            _moveSpeed = _desiredMoveSpeed;
        }

        _lastDesiredMoveSpeed = _desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(_desiredMoveSpeed - _moveSpeed);
        float startValue = _moveSpeed;

        while (time < difference)
        {
            _moveSpeed = Mathf.Lerp(startValue, _desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }

            time += Time.deltaTime;
            yield return null;
        }

        _moveSpeed = _desiredMoveSpeed;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, playerHeight * 0.5f + 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    private Vector3 velocityToSet;

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        _rb.velocity = velocityToSet;
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Legacy_GrapplingHook>().StopGrapple();
        }
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ =
            new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z); ;

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}
