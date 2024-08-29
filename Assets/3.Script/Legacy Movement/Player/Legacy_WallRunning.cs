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

    private void Update()
    {
        CheckForWall();
    }

    private void CheckForWall()
    {
        _wallRight = Physics.Raycast(transform.position, orientation.right, out _rightWallHit, wallCheckDistance, whatIsWall);
        _wallLeft = Physics.Raycast(transform.position, -orientation.right, out _leftWallHit, wallCheckDistance, whatIsWall);
        
    }
}
