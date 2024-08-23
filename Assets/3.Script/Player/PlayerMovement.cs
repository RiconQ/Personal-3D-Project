using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    private bool _readyToJump;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask gorundLayer;
    private bool _isGround;

    [SerializeField] private Transform _orientation;

    //Input System
    [HideInInspector] public PlayerInputSystem playerInputSystem;

    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        playerInputSystem = new PlayerInputSystem();
    }

    private void Start()
    {
        _rb.freezeRotation = true;
        ResetJump();
    }

    private void Update()
    {
        _isGround = IsGrounded();

        MoveInput();
        SpeedControl();

        if (_isGround)
            _rb.drag = groundDrag;
        else
            _rb.drag = 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MoveInput()
    {
        _moveInput = playerInputSystem.Player.Move.ReadValue<Vector2>().normalized;

        if (playerInputSystem.Player.Jump.triggered && _readyToJump && _isGround)
        {
            _readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        _moveDirection =
            _orientation.forward * _moveInput.y +
            _orientation.right * _moveInput.x;

        //Debug.Log($"Forward = {_orientation.forward}\nRight = {_orientation.right}");
        if (_isGround)
            _rb.AddForce(_moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else
            _rb.AddForce(_moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

    }


    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, gorundLayer);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
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
