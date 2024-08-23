using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float _moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    [Space]
    public float groundDrag;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    private bool _readyToJump;

    [Header("Crouch")]
    public float crouchSpeed;
    public float crouchYScale;
    private float _startYScale;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask gorundLayer;
    [SerializeField] private bool _isGround;

    [SerializeField] private Transform _orientation;

    //Input System
    [HideInInspector] public PlayerInputSystem playerInputSystem;

    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private Rigidbody _rb;

    public EMovementState movementState;

    public enum EMovementState
    {
        Walk,
        Sprint,
        Crouch,
        Air
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        playerInputSystem = new PlayerInputSystem();
    }

    private void Start()
    {
        _rb.freezeRotation = true;
        ResetJump();
        _startYScale = transform.localScale.y;
    }

    private void Update()
    {
        _isGround = IsGrounded();

        MoveInput();
        SpeedControl();
        StateHandler();

        if (_isGround)
            _rb.drag = groundDrag;
        else
            _rb.drag = 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void StateHandler()
    {
        //Crouch
        if (playerInputSystem.Player.Crouch.IsPressed())
        {
            movementState = EMovementState.Crouch;
            _moveSpeed = crouchSpeed;
            return;
        }
        // Sprint
        if (_isGround && playerInputSystem.Player.Sprint.IsPressed())
        {
            movementState = EMovementState.Sprint;
            _moveSpeed = sprintSpeed;
            return;
        }
        // Walk
        else if (_isGround)
        {
            movementState = EMovementState.Walk;
            _moveSpeed = walkSpeed;
            return;
        }
        // Air
        else
        {
            movementState = EMovementState.Air;
            return;
        }
    }

    private void MoveInput()
    {
        _moveInput = playerInputSystem.Player.Move.ReadValue<Vector2>().normalized;

        //Jump
        if (playerInputSystem.Player.Jump.triggered && _readyToJump && _isGround)
        {
            _readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //Crouch Hold
        if (playerInputSystem.Player.Crouch.IsPressed())
        {
            transform.localScale =
                new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if (movementState != EMovementState.Crouch)
                _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        else
        {
            transform.localScale =
                new Vector3(transform.localScale.x, _startYScale, transform.localScale.z);
        }
    }

    private void MovePlayer()
    {
        _moveDirection =
            _orientation.forward * _moveInput.y +
            _orientation.right * _moveInput.x;

        //Debug.Log($"Forward = {_orientation.forward}\nRight = {_orientation.right}");
        if (_isGround)
        {
            Debug.Log(_moveDirection);
            _rb.AddForce(_moveDirection.normalized * _moveSpeed * 10f, ForceMode.Force);
        }
        else
            _rb.AddForce(_moveDirection.normalized * _moveSpeed * 10f * airMultiplier, ForceMode.Force);

    }


    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, gorundLayer);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        if (flatVel.magnitude > _moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * _moveSpeed;
            _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        _readyToJump = true;
    }

    private void OnEnable()
    {
        playerInputSystem.Enable();
    }
    private void OnDisable()
    {
        playerInputSystem.Disable();
    }
}
