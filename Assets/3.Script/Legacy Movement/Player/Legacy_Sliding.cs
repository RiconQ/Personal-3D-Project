using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Legacy_Sliding : MonoBehaviour
{
    [Header("Reference")]
    public Transform orientation;
    public Transform playerObj;

    private Rigidbody _rb;
    private Legacy_PlayerMovement _playerMovement;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    public float slideYScale;

    private float _slideTimer;
    private float _startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.C;
    private float _horizontalInput;
    private float _verticalInput;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerMovement = GetComponent<Legacy_PlayerMovement>();

        _startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (_horizontalInput != 0 || _verticalInput != 0))
            StartSlide();
        if (Input.GetKeyUp(slideKey) && _playerMovement.isSliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (_playerMovement.isSliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        _playerMovement.isSliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        _rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);

        _slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection =
            orientation.forward * _verticalInput +
            orientation.right * _horizontalInput;

        // Sliding Normal
        if (!_playerMovement.OnSlope() || _rb.velocity.y > -0.1f)
        {
            _rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            _slideTimer -= Time.deltaTime;
        }

        // Sliding down a slope
        else
        {
            _rb.AddForce(_playerMovement.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }
        if (_slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        _playerMovement.isSliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, _startYScale, playerObj.localScale.z);
    }
}

