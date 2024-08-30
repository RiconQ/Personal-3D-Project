using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Legacy_Climbing : MonoBehaviour
{
    [Header("Reference")]
    public Transform orientation;
    public Rigidbody rb;
    public LayerMask whatIsWall;
    public Legacy_PlayerMovement _playerMovement;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float _climbTimer;

    private bool _climbing;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

    public KeyCode jumpKey = KeyCode.Space;
    public int climbJump;
    private int climbJumpLeft;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float _wallLookAngle;

    private RaycastHit _frontWallHit;
    private bool _wallFront;

    private Transform _lastWall;
    private Vector3 _lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitingWallTime;
    private float exitWallTimer;

    private void Update()
    {
        WallCheck();

        StateMachine();

        if (_climbing && !exitingWall) ClimbingMovement();
    }

    private void StateMachine()
    {
        if (_wallFront && Input.GetKey(KeyCode.W) && _wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!_climbing && _climbTimer > 0) StartClimbing();

            if (_climbTimer > 0) _climbTimer -= Time.deltaTime;
            if (_climbTimer < 0) StopClimbing();
        }
        else if(exitingWall)
        {
            if (_climbing) StopClimbing();

            if(exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0) exitingWall = false;
        }
        else
        {
            if (_climbing) StopClimbing();
        }

        if (_wallFront && Input.GetKeyDown(jumpKey) && climbJumpLeft > 0)
        {
            ClimbJump();
        }

    }

    private void WallCheck()
    {
        _wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out _frontWallHit, detectionLength, whatIsWall);
        _wallLookAngle = Vector3.Angle(orientation.forward, -_frontWallHit.normal);

        bool newWall = _frontWallHit.transform != _lastWall ||
            Mathf.Abs(Vector3.Angle(_lastWallNormal, _frontWallHit.normal)) > minWallNormalAngleChange;

        if ((_wallFront && newWall) || _playerMovement.grounded)
        {
            _climbTimer = maxClimbTime;
            climbJumpLeft = climbJump;
        }
    }

    private void StartClimbing()
    {
        _climbing = true;

        _playerMovement.isClimbing = true;
        //camera fov change

        _lastWall = _frontWallHit.transform;
        _lastWallNormal = _frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);

        // sound effect
    }

    private void StopClimbing()
    {
        _climbing = false;
        _playerMovement.isClimbing = false;
        //particle effect
    }

    private void ClimbJump()
    {
        exitingWall = true;
        exitWallTimer = exitingWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce +
            _frontWallHit.normal * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpLeft--;
    }
}
