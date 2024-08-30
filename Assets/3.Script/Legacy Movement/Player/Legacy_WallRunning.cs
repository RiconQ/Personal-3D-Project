using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Legacy_WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;

    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float _wallRunTimer;

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode upwardRunKey = KeyCode.LeftShift;
    public KeyCode downwardRunKey = KeyCode.LeftControl;

    private bool _upwardRunning;
    private bool _downwardRunning;
    private float _horizontalInput;
    private float _verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit _leftWallHit;
    private RaycastHit _rightWallHit;
    private bool _wallRight;
    private bool _wallLeft;

    [Header("Exiting")]
    private bool _exitingWall;
    public float exitWallTime;
    private float _exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    private Legacy_PlayerMovement _playerMovement;
    private Rigidbody _rb;
    public Legacy_PlayerCam playerCam;

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
        }
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void CheckForWall()
    {
        _wallRight = Physics.Raycast(transform.position, orientation.right, out _rightWallHit, wallCheckDistance, whatIsWall);
        _wallLeft = Physics.Raycast(transform.position, -orientation.right, out _leftWallHit, wallCheckDistance, whatIsWall);
        //if (_wallLeft || _wallRight)
        //    Debug.Log($"wallRight {_wallRight}, wallLeft{_wallLeft}");
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsWall);
    }

    private void StateMachine()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        _upwardRunning = Input.GetKey(upwardRunKey);
        _downwardRunning = Input.GetKey(downwardRunKey);

        //Debug.Log($"_verticalInput : {_verticalInput}, AboveGround() : {AboveGround()} ");

        if ((_wallLeft || _wallRight) && _verticalInput > 0 && AboveGround() && !_exitingWall)
        {
            //Debug.Log("StartWallrun");
            //Start Wall Run

            if (!_playerMovement.wallRunning)
                StartWallRun();

            if(_wallRunTimer > 0)
                _wallRunTimer -= Time.deltaTime;


            if (_wallRunTimer <= 0 && _playerMovement.wallRunning)
            {
                _exitingWall = true;
                _exitWallTimer = exitWallTime;
            }


            if (Input.GetKeyDown(jumpKey))
                WallJump();
        }

        else if(_exitingWall)
        {
            if (_playerMovement.wallRunning)
                StopWallRun();

            if(_exitWallTimer > 0)
                _exitWallTimer -= Time.deltaTime;

            if (_exitWallTimer <= 0)
                _exitingWall = false;
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
        _wallRunTimer = maxWallRunTime;

        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        playerCam.DoFov(90f);
        if (_wallLeft) playerCam.DoTilt(-5f);
        if (_wallRight) playerCam.DoTilt(5f);
    }

    private void WallRunningMovement()
    {
        _rb.useGravity = useGravity;

        Vector3 wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        _rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (_upwardRunning)
            _rb.velocity = new Vector3(_rb.velocity.x, wallClimbSpeed, _rb.velocity.z);
        if (_downwardRunning)
            _rb.velocity = new Vector3(_rb.velocity.x, -wallClimbSpeed, _rb.velocity.z);


        if (!(_wallLeft && _horizontalInput > 0) && !(_wallRight && _horizontalInput < 0))
            _rb.AddForce(-wallNormal * 100, ForceMode.Force);

        if (useGravity)
            _rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {

        _playerMovement.wallRunning = false;
        playerCam.DoFov(80f);
        playerCam.DoTilt(0f);
    }

    private void WallJump()
    {
        _exitingWall = true;
        _exitWallTimer = exitWallTime;

        Vector3 wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        //reset y velo
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
