using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrapplingGun : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerCharacter _playerMovement;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _gunTip;
    [SerializeField] private LayerMask _whatIsGrapple;
    [SerializeField] private LineRenderer _lr;

    [Header("Grappling")]
    [SerializeField] private float _maxGrappleDistance;
    [SerializeField] private float _grappleDelayTime;
    [SerializeField] private float _overshootYAxis;

    [Header("CoolDown")]
    [SerializeField] private float _grappleCooldown;

    // Variables
    private Vector3 _grapplePoint;
    private float _grappleCooldownTimer;
    private bool _isGrappling;

    private void Start()
    {
        _playerMovement = GetComponent<PlayerCharacter>();
        _lr.enabled = false;
    }

    private void Update()
    {
        if(_grappleCooldownTimer > 0)
            _grappleCooldownTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (_isGrappling)
            _lr.SetPosition(0, _gunTip.position);
    }

    private void StartGrapple()
    {
        if (_grappleCooldownTimer > 0) return;

        _isGrappling = true;
        //_playerMovement.isFreeze = true;

        RaycastHit hit;
        //if(Physics.Raycast(_))
    }
}
