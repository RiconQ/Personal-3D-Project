using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Legacy_WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    private float _horizontalInput;
    private float _verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit _leftWallHit;
    private RaycastHit _rightWallHit;
    private bool _wallRight;
    private bool _wallLeft;

    [Header("References")]
    public Transform orientation;
    private Legacy_PlayerMovement _playerMovement;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerMovement = GetComponent<Legacy_PlayerMovement>();
    }

    private void FixedUpdate()
    {
        if (_playerMovement.wallRunning)
        {
            WallRunningMovement();
            StateMachine();
        }
    }

    private void Update()
    {
        CheckForWall();
    }

    private void CheckForWall()
    {
        _wallRight = Physics.Raycast(transform.position, orientation.right, out _rightWallHit, wallCheckDistance, whatIsWall);
        _wallLeft = Physics.Raycast(transform.position, -orientation.right, out _leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsWall);
    }

    private void StateMachine()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if ((_wallLeft || _wallRight) && _verticalInput > 0 && AboveGround())
        {
            //Start Wall Run

            if (!_playerMovement.wallRunning)
                StartWallRun();
        }
        else
        {
            if (_playerMovement.wallRunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        _playerMovement.wallRunning = true;
    }

    private void WallRunningMovement()
    {
        _rb.useGravity = false;
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        Vector3 wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        _rb.AddForce(wallForward * wallRunForce, ForceMode.Force);


        if (!(_wallLeft && _horizontalInput > 0) && !(_wallRight && _horizontalInput < 0))
            _rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    private void StopWallRun()
    {

        _playerMovement.wallRunning = false;
    }
}
