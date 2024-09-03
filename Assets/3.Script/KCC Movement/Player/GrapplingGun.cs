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
    [SerializeField] private float _maxGrappleDistance = 25f;
    [SerializeField] private float _grappleDelayTime = 0.25f;
    [SerializeField] private float _overshootYAxis = 2f;

    [Header("CoolDown")]
    [SerializeField] private float _grappleCooldown = 0.5f;

    // Variables
    private Vector3 _grapplePoint;
    private float _grappleCooldownTimer;
    private bool _isGrappling;
    public bool isFreeze;

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

    public void UpdateRopePosition()
    {
        if (_isGrappling)
            _lr.SetPosition(0, _gunTip.position);
    }

    public void StartGrapple()
    {
        Debug.Log("Start Grapple");
        if (_grappleCooldownTimer > 0) return;

        _isGrappling = true;
        isFreeze = true;

        RaycastHit hit;
        if(Physics.Raycast(
            _cameraTransform.position, 
            _cameraTransform.forward,
            out hit, _maxGrappleDistance, _whatIsGrapple))
        {
            _grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), _grappleDelayTime);
        }
        else
        {
            _grapplePoint = _cameraTransform.position + _cameraTransform.forward * _maxGrappleDistance;
            Invoke(nameof(StopGrapple), _grappleDelayTime);
        }

        _lr.enabled = true;
        _lr.SetPosition(1, _grapplePoint);
    }

    private void ExecuteGrapple()
    {
        isFreeze = false;

        
    }

    public void StopGrapple()
    {
        isFreeze = false;
        _isGrappling = false;

        _grappleCooldownTimer = _grappleCooldown;

        _lr.enabled = false;
    }
}
